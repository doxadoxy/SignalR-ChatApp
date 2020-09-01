using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        static List<Users> allUsers = new List<Users>();
        static List<Messages> allMessage = new List<Messages>();

        private readonly static ConnectionMapping<string> _connections =  new ConnectionMapping<string>();

        public void SendMessageToAll(string userName, string message)
        {
            var currentTime = DateTime.Now.ToString("HH:mm:ss");
            allMessage.Add(new Messages { UserName = userName, Message = message, CurrentTime = currentTime });
            Clients.All.messageReceived(userName, message, currentTime);
        
        }

        public void Connect(string userName) {

            string id = Context.ConnectionId;
            string name = Context.User.Identity.Name;

            if (allUsers.Count(x => x.ConnectionId == id) == 0)
            {

                var currentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                allUsers.Add(new Users { ConnectionId = id, UserName = userName });


                Clients.Caller.onConnected(id, userName, allUsers, allMessage, currentDate);

                Clients.AllExcept(id).onNewUserConnected(id, userName);
               
            }
        }

        public void SendPrivateMessage(string toUserId, string message)
        {
            string windowId = Context.ConnectionId;
            var user = allUsers.FirstOrDefault(x => x.ConnectionId == windowId);
            if (user != null)
            {
                string fromUserName = user.UserName;
                Clients.Client(toUserId).sendPrivateMessage(windowId, fromUserName, message);
                Clients.Caller.sendPrivateMessage(toUserId, fromUserName, message);
            }
           

            //string windowId = Context.ConnectionId;
            //string fromUserName = Context.User.Identity.Name;
            //Clients.Client(toUserId).sendPrivateMessage(windowId, toUserId, message);
            //Clients.Caller.sendPrivateMessage(fromUserName, message);

        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var item = allUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                allUsers.Remove(item);

                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.UserName);

            }

            return base.OnDisconnected(stopCalled);
        }

    }
}