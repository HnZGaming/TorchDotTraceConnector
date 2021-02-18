using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Game.Multiplayer;
using Utils.General;

namespace Utils.Torch
{
    public sealed class SimDropObserver
    {
        readonly double[] _recentSims;
        int _intervalCount;

        public SimDropObserver(int bufferCount)
        {
            _recentSims = Enumerable.Repeat(1d, bufferCount).ToArray();
        }

        public async Task Main(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UpdateBuffer();

                await Task.Delay(1.Seconds(), cancellationToken);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void UpdateBuffer()
        {
            var sim = Sync.ServerSimulationRatio;
            _recentSims[_intervalCount++ % _recentSims.Length] = sim;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsLaggierThan(double sim)
        {
            return _recentSims.Average() < sim;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Reset()
        {
            _recentSims.Fill(1);
        }
    }
}