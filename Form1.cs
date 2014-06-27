using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Drbg_Test
{
    public partial class Form1 : Form
    {
        #region Enums
        private enum ApiTypes
        {
            A,
            R,
        }

        private enum TestTypes : int
        {
            Entropy = 0,
            ChiSquare,
            ChiProbability,
            Mean,
            MonteCarloErrorPct,
            MonteCarloPiCalc,
            SerialCorrelation,
        }
        #endregion

        #region Constants
        // output random to file path
        private const string RANDOMOUT_PATH = @"C:\Tests\random.bin";
        // results file name
        private const string RESULTS_NAME = "results.txt";
        // number of random sample pairs to test
        private const int TEST_ITERATIONS = 100;
        // size of random chunks (10 kib)
        private const int CHUNK_SIZE = 10240;
        // size of random sample (1 mib)
        private const int SAMPLE_SIZE = 1024000;
        #endregion

        #region Fields
        private string _dlgPath = string.Empty;
        #endregion

        #region Constructor
        public Form1()
        {
            InitializeComponent();
            btnTestAesCtr.Enabled = false;
            txtOutput.Text = "[Select a Destination Folder]";
        }
        #endregion

        #region Controls
        private void OnTestClick(object sender, EventArgs e)
        {
            pbStatus.Maximum = TEST_ITERATIONS;
            pbStatus.Value = 0;
            lblStatus.Text = "Processing..";
            lblStatus.Update();
            btnTestAesCtr.Enabled = false;
            grpBox.Enabled = false;

            // run the compare
            CompareApi(TEST_ITERATIONS, _dlgPath);

            grpBox.Enabled = true;
            lblStatus.Text = "Completed!";
            pbStatus.Value = TEST_ITERATIONS;/**/

            // unrem to write aesctr random to file
            //Write(GetRandom2(10240000), RANDOMOUT_PATH);
            // unrem to test instance equality
            //CompareEqual();
        }

        private void btnDialog_Click(object sender, EventArgs e)
        {
            btnTestAesCtr.Enabled = false;
            txtOutput.Text = "[Select a Destination Folder]";
            pbStatus.Value = 0;

            using (FolderBrowserDialog fbDiag = new FolderBrowserDialog())
            {
                fbDiag.Description = "Select a Folder";

                if (fbDiag.ShowDialog() == DialogResult.OK)
                {
                    if (Directory.Exists(fbDiag.SelectedPath))
                        _dlgPath = Path.Combine(fbDiag.SelectedPath, RESULTS_NAME);
                    if (File.Exists(_dlgPath))
                        File.Delete(_dlgPath);

                    txtOutput.Text = _dlgPath;
                    btnTestAesCtr.Enabled = true;
                }
            }
        }
        #endregion

        #region Tests
        private void CompareApi(int Iterations, string Path)
        {
            int percentA = 0;
            int percentR = 0;
            double entropyAvgA = 0;
            double entropyAvgR = 0;
            double chiProbA = 0;
            double chiProbR = 0;
            double chiSquareA = 0;
            double chiSquareR = 0;
            double montePiA = 0;
            double montePiR = 0;
            double monteErrA = 0;
            double monteErrR = 0;
            double meanA = 0;
            double meanR = 0;
            double serA = 0;
            double serR = 0;
            int chiProbWon = 0;
            int chiSquareWon = 0;
            int meanWon = 0;
            int monteErrWon = 0;
            int montePiWon = 0;
            int serCorWon = 0;
            ApiTypes api = ApiTypes.A;

            for (int i = 0; i < Iterations; i++)
            {
                Ent ent1 = new Ent();
                Ent ent2 = new Ent();
                EntResult res1 = ent1.Calculate(GetRandom2(SAMPLE_SIZE));
                EntResult res2 = ent2.Calculate(GetRNGRandom(SAMPLE_SIZE));
                ApiTypes bestPerc = EntCompare(res1, res2, TestTypes.Entropy);

                if (bestPerc == ApiTypes.A)
                    percentA++;
                else
                    percentR++;

                chiProbA += res1.ChiProbability;
                chiProbR += res2.ChiProbability;
                chiSquareA += res1.ChiSquare;
                chiSquareR += res2.ChiSquare;
                entropyAvgA += res1.Entropy;
                entropyAvgR += res2.Entropy;
                monteErrA += res1.MonteCarloErrorPct;
                monteErrR += res2.MonteCarloErrorPct;
                montePiA += res1.MonteCarloPiCalc;
                montePiR += res2.MonteCarloPiCalc;
                meanA += res1.Mean;
                meanR += res2.Mean;
                serA += res1.SerialCorrelation;
                serR += res2.SerialCorrelation;

                // flag the test winner (A or R)
                Append("Entropy Alg:" + bestPerc.ToString() + " AesCtr:" + res1.Entropy.ToString() + " RngApi: " + res2.Entropy.ToString(), Path);

                api = EntCompare(res1, res2, TestTypes.ChiProbability);
                if (api == ApiTypes.A) chiProbWon++;
                Append("ChiProbability: " + api.ToString(), Path);
                api = EntCompare(res1, res2, TestTypes.ChiSquare);
                if (api == ApiTypes.A) chiSquareWon++;
                Append("ChiSquare: " + api.ToString(), Path);
                api = EntCompare(res1, res2, TestTypes.Mean);
                if (api == ApiTypes.A) meanWon++;
                Append("Mean: " + api.ToString(), Path);
                api = EntCompare(res1, res2, TestTypes.MonteCarloErrorPct);
                if (api == ApiTypes.A) monteErrWon++;
                Append("MonteCarloErrorPct: " + api.ToString(), Path);
                api = EntCompare(res1, res2, TestTypes.MonteCarloPiCalc);
                if (api == ApiTypes.A) montePiWon++;
                Append("MonteCarloPiCalc: " + api.ToString(), Path);
                api = EntCompare(res1, res2, TestTypes.SerialCorrelation);
                if (api == ApiTypes.A) serCorWon++;
                Append("SerialCorrelation: " + api.ToString(), Path);
                Append(" ", Path);

                // Aes scores
                Append("AesCtr", Path);
                Append("ChiProbability: " + res1.ChiProbability.ToString(), Path);
                Append("ChiSquare: " + res1.ChiSquare.ToString(), Path);
                Append("Mean: " + res1.Mean.ToString(), Path);
                Append("MonteCarloErrorPct: " + res1.MonteCarloErrorPct.ToString(), Path);
                Append("MonteCarloPiCalc: " + res1.MonteCarloPiCalc.ToString(), Path);
                Append("SerialCorrelation: " + res1.SerialCorrelation.ToString(), Path);
                Append(" ", Path);
                // rng scores
                Append("RngApi", Path);
                Append("ChiProbability: " + res2.ChiProbability.ToString(), Path);
                Append("ChiSquare: " + res2.ChiSquare.ToString(), Path);
                Append("Mean: " + res2.Mean.ToString(), Path);
                Append("MonteCarloErrorPct: " + res2.MonteCarloErrorPct.ToString(), Path);
                Append("MonteCarloPiCalc: " + res2.MonteCarloPiCalc.ToString(), Path);
                Append("SerialCorrelation: " + res2.SerialCorrelation.ToString(), Path);
                Append("#######################################################", Path);
                Append(" ", Path);

                pbStatus.Value = i;
            }

            // get the averages
            int sumA = chiProbWon + chiSquareWon + meanWon + monteErrWon + montePiWon + serCorWon;
            int sumR = (Iterations * 6) - sumA;

            Append("Best Entropy: Aes: " + percentA.ToString() + " Rng: " + percentR.ToString(), Path);
            Append("Entropy Avg: Aes: " + (entropyAvgA / Iterations).ToString() + " Rng: " + (entropyAvgR / Iterations).ToString(), Path);
            Append("Total Tests Score: Aes: " + sumA.ToString() + " Rng: " + sumR.ToString(), Path);
            Append("ChiProbability Score: Aes: " + chiProbWon.ToString() + " Rng: " + (Iterations - chiProbWon).ToString(), Path);
            Append("ChiProbability Avg: Aes: " + (chiProbA / Iterations).ToString() + " Rng: " + (chiProbR / Iterations).ToString(), Path);
            Append("ChiSquare Score: Aes: " + chiSquareWon.ToString() + " Rng: " + (Iterations - chiSquareWon).ToString(), Path);
            Append("ChiSquare Avg: Aes: " + (chiSquareA / Iterations).ToString() + " Rng: " + (chiSquareR / Iterations).ToString(), Path);
            Append("MonteCarlo Error Score: Aes: " + monteErrWon.ToString() + " Rng: " + (Iterations - monteErrWon).ToString(), Path);
            Append("MonteCarlo Error Avg: Aes: " + (monteErrA / Iterations).ToString() + " Rng: " + (monteErrR / Iterations).ToString(), Path);
            Append("MonteCarlo Pi Score: Aes: " + montePiWon.ToString() + " Rng: " + (Iterations - montePiWon).ToString(), Path);
            Append("MonteCarlo Pi Avg: Aes: " + (montePiA / Iterations).ToString() + " Rng: " + (montePiR / Iterations).ToString(), Path);
            Append("Mean Score: Aes: " + meanWon.ToString() + " Rng: " + (Iterations - meanWon).ToString(), Path);
            Append("Mean Avg: Aes: " + (meanA / Iterations).ToString() + " Rng: " + (meanR / Iterations).ToString(), Path);
            Append("Serial Correlation Score: Aes: " + serCorWon.ToString() + " Rng: " + (Iterations - serCorWon).ToString(), Path);
            Append("Serial Correlation Avg: Aes: " + (serA / Iterations).ToString() + " Rng: " + (serR / Iterations).ToString(), Path);
        }

        private ApiTypes EntCompare(EntResult Res1, EntResult Res2, TestTypes TestType = TestTypes.Entropy)
        {
            const double MEAN = 127.5;
            const double PI = 3.1415926535;

            ApiTypes winner = ApiTypes.A;

            switch (TestType)
            {
                case TestTypes.Entropy:
                    {
                        if (Res2.Entropy > Res1.Entropy)
                            winner = ApiTypes.R;
                    }
                    break;
                case TestTypes.ChiProbability:
                    {
                        if (Math.Abs(Res1.ChiProbability - 0.5) > Math.Abs(Res2.ChiProbability - 0.5))
                            winner = ApiTypes.R;
                    }
                    break;
                case TestTypes.ChiSquare:
                    {
                        if (Math.Abs(Res1.ChiSquare - 256.0) > Math.Abs(Res2.ChiSquare - 256.0))
                            winner = ApiTypes.R;
                    }
                    break;
                case TestTypes.Mean:
                    {
                        if (Math.Abs(MEAN - Res1.Mean) > Math.Abs(MEAN - Res2.Mean))
                            winner = ApiTypes.R;
                    }
                    break;
                case TestTypes.MonteCarloErrorPct:
                    {
                        if (Math.Abs(Res1.MonteCarloErrorPct)  > Math.Abs(Res2.MonteCarloErrorPct))
                            winner = ApiTypes.R;
                    }
                    break;
                case TestTypes.MonteCarloPiCalc:
                    {
                        if (Math.Abs(PI - Res1.MonteCarloPiCalc) > Math.Abs(PI - Res2.MonteCarloPiCalc))
                            winner = ApiTypes.R;
                    }
                    break;
                case TestTypes.SerialCorrelation:
                    {
                        if (Math.Abs(Res1.SerialCorrelation) > Math.Abs(Res2.SerialCorrelation))
                            winner = ApiTypes.R;
                    }
                    break;

            }
            return winner;
        }

        private void CompareEqual()
        {
            byte[] data = new byte[1024000];
            byte[] data2 = new byte[1024000];
            byte[] seed = GetSeed();

            using (AesCtr ctr = new AesCtr())
                data = ctr.Generate(seed, 1024000);
            using (AesCtr ctr = new AesCtr())
                data2 = ctr.Generate(seed, 1024000);

            if (!Equal(data, data2))
                throw new Exception("Arrays are not equal!");
        }

        #endregion

        #region Algorithms
        /// <summary>
        /// Get random using seed and key entropy
        /// </summary>
        private byte[] GetRandom(int Size)
        {
            byte[] output = new byte[Size];

            for (int i = 0; i < Size; i += CHUNK_SIZE)
            {
                byte[] data = new byte[CHUNK_SIZE];
                byte[] seed = GetSeed();
                byte[] key = GetKey();

                using (AesCtr ctr = new AesCtr())
                    data = ctr.Generate(seed, key, CHUNK_SIZE);

                Buffer.BlockCopy(data, 0, output, i, CHUNK_SIZE);
            }

            return output;
        }

        /// <summary>
        /// Get random using seed
        /// </summary>
        private byte[] GetRandom2(int Size)
        {
            byte[] output = new byte[Size];

            for (int i = 0; i < Size; i += CHUNK_SIZE)
            {
                byte[] data = new byte[CHUNK_SIZE];
                byte[] seed = GetSeed();

                using (AesCtr ctr = new AesCtr())
                    data = ctr.Generate(seed, CHUNK_SIZE);

                Buffer.BlockCopy(data, 0, output, i, CHUNK_SIZE);
            }

            return output;
        }

        /// <summary>
        /// Get random using auto seeding
        /// </summary>
        private byte[] GetRandom3(int Size)
        {
            byte[] output = new byte[Size];

            for (int i = 0; i < Size; i += CHUNK_SIZE)
            {
                byte[] data = new byte[CHUNK_SIZE];

                using (AesCtr ctr = new AesCtr())
                    data = ctr.Generate(CHUNK_SIZE);

                Buffer.BlockCopy(data, 0, output, i, CHUNK_SIZE);
            }

            return output;
        }

        private byte[] GetRNGRandom(int Size)
        {
            byte[] output = new byte[Size];
            using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
                random.GetBytes(output);

            return output;
        }
        #endregion

        #region Helpers
        private void Append(string Data, string Path)
        {
            using (StreamWriter st = File.AppendText(Path))
                st.WriteLine(Data);
        }

        private void Write(byte[] Data, string Path)
        {
            using (BinaryWriter br = new BinaryWriter(File.Open(Path, FileMode.Create)))
                br.Write(Data);
        }

        private bool Equal(byte[] b1, byte[] b2)
        {
            return b1.SequenceEqual(b2);
        }

        private byte[] GetSeed()
        {
            using (RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[128];
                byte[] data2 = new byte[128];
                byte[] ret = new byte[64];

                rand.GetBytes(data);
                rand.GetBytes(data2);

                // entropy extractor
                using (SHA256 shaHash = SHA256Managed.Create())
                {
                    Buffer.BlockCopy(shaHash.ComputeHash(data), 0, ret, 0, 32);
                    Buffer.BlockCopy(shaHash.ComputeHash(data2), 0, ret, 32, 32);
                    return ret;
                }
            }
        }

        private byte[] GetKey()
        {
            using (RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[128];
                rand.GetBytes(data);

                // entropy extractor
                using (SHA256 shaHash = SHA256Managed.Create())
                    return shaHash.ComputeHash(data);
            }
        }
        #endregion
    }
}
