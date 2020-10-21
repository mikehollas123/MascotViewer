using System;
using System.Collections.Generic;

using matrix_science.msparser;

namespace MascotViewer
{
    public class MascotReader : IDisposable
    {
        private ms_mascotresfile _mascotFile;
        private string _FileName;
        private ms_searchparams _searchParams;
        private ms_mascotoptions _mascotOptions;

        public string FileName
        {
            get { return _FileName; }

        }

        public MascotReader(string datFile)
        {
            this._mascotFile = new ms_mascotresfile(datFile);
            this._searchParams = new ms_searchparams(_mascotFile);
            this._FileName = System.IO.Path.GetFileNameWithoutExtension(_searchParams.getFILENAME());
            this._mascotOptions = new ms_datfile(datFile).getMascotOptions();
        }


        public List<IProtein> GetProteinsWithPeptides()
        {
            uint flags, flags2, minPepLenInPepSummary;
            int maxHitsToReport;
            double minProbability, ignoreIonsScoreBelow;
            bool usePeptideSummary;

            string scriptName = this._mascotFile.get_ms_mascotresults_params(
                                        this._mascotOptions,
                                        out flags,
                                        out minProbability,
                                        out maxHitsToReport,
                                        out ignoreIonsScoreBelow,
                                        out minPepLenInPepSummary,
                                        out usePeptideSummary,
                                        out flags2);
            ms_mascotresults msSummary;
            if (usePeptideSummary)
            {
                msSummary = new ms_peptidesummary(this._mascotFile, flags,
                   minProbability,
                   maxHitsToReport,
                   "", //unigene file
                   ignoreIonsScoreBelow,
                   (int)minPepLenInPepSummary,
                   null,
                   flags2);
            }
            else
            {
                msSummary = new ms_proteinsummary(_mascotFile, flags,
                                minProbability,
                                 maxHitsToReport,
                                  null,
                                  null);
            }

            var totalNumHits = msSummary.getNumberOfHits();
            var proteins = new List<IProtein>(totalNumHits);

            for (int i = 1; i <= totalNumHits; i++)
            {
                var prot = msSummary.getHit(i);

                var accession = prot.getAccession();
                var count = prot.getNumPeptides();
                var description = msSummary.getProteinDescription(accession);
                var mass = msSummary.getProteinMass(accession);
                var coverage = prot.getCoverage();
                var score = prot.getScore();
                var RMS = prot.getRMSDeltas(msSummary);

                Peptide[] peptides = new Peptide[count];
                int query;
                ms_peptide pep;
                for (int j = 1; j <= count; j++)
                {
                    query = prot.getPeptideQuery(j);
                    var p = prot.getPeptideP(j);
                    if (p != -1 && query != -1 && prot.getPeptideDuplicate(j) != ms_protein.DUPLICATE.DUPE_DuplicateSameQuery)
                    {
                        pep = msSummary.getPeptide(query, p);
                        if (pep != null)
                        {
                            peptides[j - 1] = getPeptideInfo(pep, msSummary, prot.getPeptideDuplicate(j) == ms_protein.DUPLICATE.DUPE_Duplicate,
                                prot.getPeptideIsBold(j), prot.getPeptideShowCheckbox(j));
                        }
                    }
                }





                proteins.Add(new Protein()
                {
                    Peptides = peptides,
                    Accession = accession,
                    PeptideCount = count,
                    Mass = mass,
                    Description = description,
                    Coverage = coverage,
                    RMSError = RMS
                });
            }

            return proteins;
        }


        public List<IProtein> GetProteins()
        {
            uint flags, flags2, minPepLenInPepSummary;
            int maxHitsToReport;
            double minProbability, ignoreIonsScoreBelow;
            bool usePeptideSummary;

            string scriptName = this._mascotFile.get_ms_mascotresults_params(
                                        this._mascotOptions,
                                        out flags,
                                        out minProbability,
                                        out maxHitsToReport,
                                        out ignoreIonsScoreBelow,
                                        out minPepLenInPepSummary,
                                        out usePeptideSummary,
                                        out flags2);
            ms_mascotresults msSummary;
            if (usePeptideSummary)
            {
                msSummary = new ms_peptidesummary(this._mascotFile, flags,
                   minProbability,
                   maxHitsToReport,
                   "", //unigene file
                   ignoreIonsScoreBelow,
                   (int)minPepLenInPepSummary,
                   null,
                   flags2);
            }
            else
            {
                msSummary = new ms_proteinsummary(_mascotFile, flags,
                                minProbability,
                                 maxHitsToReport,
                                  null,
                                  null);
            }

            var totalNumHits = msSummary.getNumberOfHits();
            var proteins = new List<IProtein>(totalNumHits);

            for (int i = 1; i <= totalNumHits; i++)
            {
                var prot = msSummary.getHit(i);

                var accession = prot.getAccession();
                var count = prot.getNumPeptides();
                var description = msSummary.getProteinDescription(accession);
                var mass = msSummary.getProteinMass(accession);
                var coverage = prot.getCoverage();
                var score = prot.getScore();
                var RMS = prot.getRMSDeltas(msSummary);

                proteins.Add(new SmallProtein()
                {
                 
                    Accession = accession,
                    PeptideCount = count,
                    Mass = mass,
                    Description = description
                });
            }

            return proteins;
        }

        private Peptide getPeptideInfo(ms_peptide p, ms_mascotresults r, bool isDuplicate, bool isBold, bool showCheckBox)
        {
            int q = p.getQuery();

            if (p.getAnyMatch())
            {


                return new Peptide
                {
                    Rank = p.getRank(),
                    Sequence = p.getPeptideStr(),
                    MissedCleaveges = p.getMissedCleavages(),
                    Mods = r.getReadableVarMods(q, p.getRank(), 2),
                    NumIonsMatched = p.getNumIonsMatched(),
                    ObservedMz = p.getObserved(),
                    Delta = p.getDelta(),
                    MrCalc = p.getMrCalc(),
                    MrExperimental = p.getMrExperimental(),
                    Charge = p.getCharge()
                };
            }
            else
            {
                return null;
            }
        }


        public void Dispose()
        {
            _mascotFile.Dispose();
        }
    }

    public interface IProtein
    {
        string Accession { get; set; }
        string Description { get; set; }
         double Mass { get; set; }
        double PeptideCount { get; set; }
    }
    public class Protein : IProtein
    {
        public string Accession { get; set; }
        public string Description { get; set; }
        public double Mass { get; set; }
        public double PeptideCount { get; set; }
        public Peptide[] Peptides { get; set; }
        public double Score { get; set; }
        public double RMSError { get; set; }

        public double Coverage { get; set; }

    }
    public class SmallProtein : IProtein
    {
        public string Accession { get; set; }
        public string Description { get; set; }
        public double Mass { get; set; }
        public double PeptideCount { get; set; }
       
    }

    public class Peptide
    {
        public string Sequence { get; set; }
        public int Rank { get; set; }
        public int MissedCleaveges { get; set; }
        public double Delta { get; set; }
        public double ObservedMz { get; set; }
        public int Charge { get; set; }

        public double MrExperimental { get; set; }
        public int NumIonsMatched { get; set; }
        public string Mods { get; set; }
        public double MrCalc { get; set; }


    }
}
