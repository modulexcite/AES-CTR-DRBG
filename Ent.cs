/// A version of Brett Trotter's C# version of Ent (Thanks Brett!): http://www.codeproject.com/Articles/11672/ENT-A-Pseudorandom-Number-Sequence-Test-Program-C?msg=4671947#xx4671947xx
/// The original c++ program written by John Walker (Thanks John!): http://www.fourmilab.ch/random/
/// Rewritten with optimizations for speed..
/*
random.org
C:\Tests\Test OTP>ent random.bin
Entropy = 7.999827 bits per byte.
Optimum compression would reduce the size of this 1048648 byte file by 0 percent.
Chi square distribution for 1048648 samples is 252.09, and randomly would exceed this value 53.97 percent of the times.
Arithmetic mean value of data bytes is 127.5177 (127.5 = random).
Monte Carlo value for Pi is 3.141519906 (error 0.00 percent).
Serial correlation coefficient is -0.001317 (totally uncorrelated = 0.0).

vpad
C:\Tests\Test OTP>ent vrandom.bin
Entropy = 7.999826 bits per byte.
Optimum compression would reduce the size of this 1048610 byte file by 0 percent.
Chi square distribution for 1048610 samples is 253.25, and randomly would exceed this value 51.91 percent of the times.
Arithmetic mean value of data bytes is 127.4960 (127.5 = random).
Monte Carlo value for Pi is 3.144648906 (error 0.10 percent).
Serial correlation coefficient is 0.000145 (totally uncorrelated = 0.0).
*/
using System;
using System.IO;
using System.ComponentModel;

namespace Drbg_Test
{
    internal class EntResult
    {
        internal double Entropy;
        internal double ChiSquare;
        internal double Mean;
        internal double MonteCarloPiCalc;
        internal double SerialCorrelation;
        internal long[] OccuranceCount;
        internal double ChiProbability;
        internal double MonteCarloErrorPct;
        internal double OptimumCompressionReductionPct;
        internal double ExpectedMeanForRandom;
        internal long NumberOfSamples;
        internal double[] PiSamples;
        internal double[] MeanSamples;
    }

    #region Delegate
    internal delegate void EntCounterDelegate(long percent);
    #endregion

    internal class Ent
    {
        #region Event
        internal event EntCounterDelegate ProgressCounter;
        #endregion

        #region Fields
        private double[,] chsqt = new double[2, 10] 
			{
				{0.5, 0.25, 0.1, 0.05, 0.025, 0.01, 0.005, 0.001, 0.0005, 0.0001}, 
				{0.0, 0.6745, 1.2816, 1.6449, 1.9600, 2.3263, 2.5758, 3.0902, 3.2905, 3.7190}
			};
        /// Bytes used as Monte Carlo co-ordinates
		/// This should be no more bits than the mantissa 
		/// of your "double" floating point type.
        private static int MONTEN = 6;				
        private uint[] _MonteCarlo = new uint[MONTEN];
        /// Probabilities per bin for entropy
        private double[] _Probability = new double[256];
        /// Bins to count occurrences of values
        private long[] _BinCount = new long[256];
        /// Total bytes counted
        private long _TotalCount = 0;
        private static double _currentProgress = 0;
        private long _InMont, _MCount;
        private double _InCirc;
        private double _MonteX, _MonteY, _MontePi;
        private double SCC, SCCRUN, SCCU0, SCCLAST, SCCT1, SCCT2, SCCT3;
        private double[] _piSamples = new double[SUBSAMPLES];
        private double[] _meanSamples = new double[SUBSAMPLES];
        private const int SUBSAMPLES = 64;
        private const int SAMPLESIZE = 4096;
        private const int BINBUFFER = 32768;
        #endregion

        #region Constructor
        internal Ent()
        {
            Init();
        }
        #endregion

        #region Public Methods
        internal EntResult Calculate(string FileName)
        {
            byte[] fileBuffer;
            _currentProgress = 0;

            using (FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                fileBuffer = new byte[fileStream.Length];
                fileStream.Read(fileBuffer, 0, (int)fileStream.Length);
            }

            AddSamples(fileBuffer);

            return EndCalculation();
        }

        internal EntResult Calculate(byte[] Buffer)
        {
            _currentProgress = 0;
            AddSamples(Buffer);

            return EndCalculation();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Calculate the progress
        /// </summary>
        /// <param name="Position">Current position</param>
        /// <param name="Maximum">Progress max</param>
        private void CalculateProgress(long Position, long Maximum)
        {
            if (ProgressCounter != null)
            {
                double pos = Position;
                double percent = Math.Round((double)(pos / Maximum) * 100, 0);
                if (percent > _currentProgress)
                {
                    ProgressCounter((long)percent);
                    _currentProgress = percent;
                }
            }
        }

        /// <summary>
        /// Initialize random counters
        /// </summary>
        /// <param name="BinaryMode">Binary mode</param>
        private void Init()
        {  						
            // Reset Monte Carlo accumulator pointer
            _MCount = 0;						
            // Clear Monte Carlo tries
            _InMont = 0;						
            // Clear Monte Carlo inside count
            _InCirc = 65535.0 * 65535.0;					
            // Mark first time for serial correlation
            SCCT1 = SCCT2 = SCCT3 = 0.0;
            // Clear serial correlation terms
            _InCirc = Math.Pow(Math.Pow(256.0, (double)(MONTEN / 2)) - 1, 2.0);

            for (int i = 0; i < 256; i++)
                _BinCount[i] = 0;

            _TotalCount = 0;
        }

        /// <summary>
        /// Add one or more bytes to accumulation
        /// </summary>
        /// <param name="Samples">Buffer</param>
        /// <param name="Fold">Fold - not implemented</param>
        private void AddSamples(byte[] Samples)
        {
            int mp = 0;
            bool sccFirst = true;
            int preProcessLength = (Samples.Length - BINBUFFER) / SAMPLESIZE;
            int counter = 0;

            _piSamples = new double[preProcessLength];
            _meanSamples = new double[preProcessLength];

            for (int i = 0; i < Samples.Length; i++)
            {
                // Update counter for this bin
                _BinCount[(int)Samples[i]]++;
                _TotalCount++;
                // Update inside/outside circle counts for Monte Carlo computation of PI
                _MonteCarlo[mp++] = Samples[i];

                // Save character for Monte Carlo
                if (mp >= MONTEN)
                {
                    // Calculate every MONTEN character
                    int mj;
                    mp = 0;
                    _MCount++;
                    _MonteX = _MonteY = 0;

                    for (mj = 0; mj < MONTEN / 2; mj++)
                    {
                        _MonteX = (_MonteX * 256.0) + _MonteCarlo[mj];
                        _MonteY = (_MonteY * 256.0) + _MonteCarlo[(MONTEN / 2) + mj];
                    }

                    if ((_MonteX * _MonteX + _MonteY * _MonteY) <= _InCirc)
                        _InMont++;
                }

                // Update calculation of serial correlation coefficient
                SCCRUN = (int)Samples[i];
                if (sccFirst)
                {
                    sccFirst = false;
                    SCCLAST = 0;
                    SCCU0 = SCCRUN;
                }
                else
                {
                    SCCT1 = SCCT1 + SCCLAST * SCCRUN;
                }

                SCCT2 = SCCT2 + SCCRUN;
                SCCT3 = SCCT3 + (SCCRUN * SCCRUN);
                SCCLAST = SCCRUN;

                // collect samples for graphs
                if (i % SAMPLESIZE == 0 && i > BINBUFFER)
                {
                    double dataSum = 0.0;

                    for (int j = 0; j < 256; j++)
                        dataSum += ((double)j) * _BinCount[j];

                    _meanSamples[counter] = dataSum / _TotalCount;
                    _piSamples[counter] = 4.0 * (((double)_InMont) / _MCount);
                    counter++;
                }
                if (i == Samples.Length - 1)
                {
                    byte[] b = new byte[16];
                    Buffer.BlockCopy(Samples, Samples.Length - 17, b, 0, 16);
                }
                CalculateProgress(_TotalCount, Samples.Length);
            }
        }

        /// <summary>
        /// Complete calculation and return results
        /// </summary>
        /// <returns>EntResult Structure</returns>
        private EntResult EndCalculation()
        {
            double entropy = 0.0;
            double chiSq = 0.0; 
            double dataSum = 0.0;
            double binVal = 0.0;
            int pos = 0;

            // Complete calculation of serial correlation coefficient
            SCCT1 = SCCT1 + SCCLAST * SCCU0;
            SCCT2 = SCCT2 * SCCT2;
            SCC = _TotalCount * SCCT3 - SCCT2;

            if (SCC == 0.0)
                SCC = -100000;
            else
                SCC = (_TotalCount * SCCT1 - SCCT2) / SCC;

            // Scan bins and calculate probability for each bin and Chi-Square distribution
            double cExp = _TotalCount / 256.0;  

            // Expected count per bin
            for (int i = 0; i < 256; i++)
            {
                _Probability[i] = (double)_BinCount[i] / _TotalCount;
                binVal = _BinCount[i] - cExp;
                chiSq = chiSq + (binVal * binVal) / cExp;
                dataSum += ((double)i) * _BinCount[i];
            }

            // Calculate entropy
            for (int i = 0; i < 256; i++)
            {
                if (_Probability[i] > 0.0)
                    entropy += _Probability[i] * Log2(1 / _Probability[i]);
            }

            // Calculate Monte Carlo value for PI from percentage of hits within the circle
            _MontePi = 4.0 * (((double)_InMont) / _MCount);

            // Calculate probability of observed distribution occurring from the results of the Chi-Square test
            double chip = Math.Sqrt(2.0 * chiSq) - Math.Sqrt(2.0 * 255.0 - 1.0);

            binVal = Math.Abs(chip);

            for (pos = 9; pos >= 0; pos--)
            {
                if (chsqt[1, pos] < binVal)
                    break;
            }

            chip = (chip >= 0.0) ? chsqt[0, pos] : 1.0 - chsqt[0, pos];
            double compReductionPct = (8 - entropy) / 8.0;

            // Return results
            EntResult result = new EntResult()
            {
                Entropy = entropy,
                ChiSquare = chiSq,
                ChiProbability = chip,
                Mean = dataSum / _TotalCount,
                ExpectedMeanForRandom = 127.5,
                MonteCarloPiCalc = _MontePi,
                MonteCarloErrorPct = (Math.Abs(Math.PI - _MontePi) / Math.PI),
                SerialCorrelation = SCC,
                OptimumCompressionReductionPct = compReductionPct,
                OccuranceCount = this._BinCount,
                NumberOfSamples = this._TotalCount,
                MeanSamples = this._meanSamples,
                PiSamples = this._piSamples
            };

            return result;
        }

        /// <summary>
        /// Returns log faction
        /// </summary>
        private double Log2(double x)
        {
            return Math.Log(x, 2);
        }
        #endregion
    }
}
