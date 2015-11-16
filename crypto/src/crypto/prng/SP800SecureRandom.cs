﻿using System;

using Org.BouncyCastle.Crypto.Prng.Drbg;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Prng
{
    public class SP800SecureRandom
        :   SecureRandom
    {
        private readonly IDrbgProvider  mDrbgProvider;
        private readonly bool           mPredictionResistant;
        private readonly SecureRandom   mRandomSource;
        private readonly IEntropySource mEntropySource;

        private ISP80090Drbg mDrbg;

        internal SP800SecureRandom(SecureRandom randomSource, IEntropySource entropySource, IDrbgProvider drbgProvider, bool predictionResistant)
            : base((IRandomGenerator)null)
        {
            this.mRandomSource = randomSource;
            this.mEntropySource = entropySource;
            this.mDrbgProvider = drbgProvider;
            this.mPredictionResistant = predictionResistant;
        }

        public override void SetSeed(byte[] seed)
        {
            lock (this)
            {
                if (mRandomSource != null)
                {
                    this.mRandomSource.SetSeed(seed);
                }
            }
        }

        public override void SetSeed(long seed)
        {
            lock (this)
            {
                // this will happen when SecureRandom() is created
                if (mRandomSource != null)
                {
                    this.mRandomSource.SetSeed(seed);
                }
            }
        }

        public override void NextBytes(byte[] bytes)
        {
            lock (this)
            {
                if (mDrbg == null)
                {
                    mDrbg = mDrbgProvider.Get(mEntropySource);
                }

                // check if a reseed is required...
                if (mDrbg.Generate(bytes, null, mPredictionResistant) < 0)
                {
                    mDrbg.Reseed(null);
                    mDrbg.Generate(bytes, null, mPredictionResistant);
                }
            }
        }

        public override void NextBytes(byte[] buf, int off, int len)
        {
            byte[] bytes = new byte[len];
            NextBytes(bytes);
            Array.Copy(bytes, 0, buf, off, len);
        }

        public override byte[] GenerateSeed(int numBytes)
        {
            byte[] bytes = new byte[numBytes];
            NextBytes(bytes);
            return bytes;
        }
    }
}
