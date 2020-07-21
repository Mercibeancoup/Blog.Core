using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Common.Hubs
{
    public interface IChatClient
    {
        /// <summary>
        /// SignalR接收信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task ReceiveMessage(object message);

        Task ReceiveMessage(string user, string message);


        Task ReceiveUpdate(object message);
    }
}
