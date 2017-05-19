using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using RabbitCloud.Rpc.Cluster.LoadBalance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RabbitCloud.Rpc.Test
{
    public class EmptyCaller : ICaller
    {
        public int Id { get; }

        public EmptyCaller(int id)
        {
            Id = id;
        }

        #region Implementation of ICaller

        public bool IsAvailable { get; } = true;

        public Task<IResponse> CallAsync(IRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion Implementation of ICaller
    }

    public class RoundRobinLoadBalanceTest
    {
        private readonly ILoadBalance _loadBalance = new RoundRobinLoadBalance();

        [Fact]
        public void SelectTest()
        {
            var callerArray = Enumerable.Range(1, 10).Select(id => new EmptyCaller(id)).ToArray();
            var callers = new List<EmptyCaller>();
            for (var i = 0; i < 13; i++)
            {
                callers.Add((EmptyCaller)_loadBalance.Select(callerArray, null));
            }

            Check(callers.ToArray(), 10);

            void Check(IReadOnlyList<EmptyCaller> array, int maxId)
            {
                for (var i = 0; i < array.Count - 1; i++)
                {
                    var current = array[i];
                    var next = array[i + 1];

                    var equalId = current.Id + 1;
                    if (equalId > maxId)
                        equalId = 1;

                    Assert.Equal(next.Id, equalId);
                }
            }
        }
    }
}