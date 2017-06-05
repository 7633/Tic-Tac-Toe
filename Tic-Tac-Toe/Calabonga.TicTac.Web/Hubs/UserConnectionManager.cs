using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Ajax.Utilities;

namespace Calabonga.TicTac.Web
{

    public sealed class UserConnectionManager
    {

        private static readonly Lazy<UserConnectionManager> Lazy = new Lazy<UserConnectionManager>(() => new UserConnectionManager());

        public static UserConnectionManager Instance { get { return Lazy.Value; } }

        private readonly List<IConnectedUser> _users = new List<IConnectedUser>();

        public IEnumerable<IConnectedUser> Users
        {
            get { return _users; }
        }

        /// <summary>
        /// ���������� true ���� ������������ �������� � ������ ��� false ���� ���������� �����������
        /// </summary>
        /// <param name="connectionId">������������� �����������</param>
        /// <param name="agentType">��� �����������</param>
        /// <param name="userName">��� ������������ (login)</param>
        /// <returns>��/���</returns>
        public bool ConnectUser(string connectionId, string agentType, string userName, string locale)
        {
            var user = new ConnectedUser
            {
                ConnectedAt = DateTime.Now,
                FullName = userName
            };
            user.RegisterConnection(connectionId, agentType, locale);
            var item = GetConnectedUserByName(userName);
            if (item != null)
            {
                item.RegisterConnection(connectionId, agentType, locale);
                return false;
            }
            _users.Add(user);
            return true;
        }

        /// <summary>
        /// ���������� ������������� ��������� ���� ������������ ������ � ������ ������������.
        /// ���� ���������� ����������� ������ ��� 1, �� ���������� ����������� � ����������� ������������� ���������.
        /// </summary>
        /// <param name="connectionId">������������� �����������</param>
        /// <returns><see cref="IConnectedUser"/></returns>
        public bool DisconnectUser(string connectionId)
        {
            var item = GetConnectedUserById(connectionId);
            if (item == null) return false;
            if (item.Connections.Any())
            {
                if (item.Connections.Count() == 1 && item.Connections.Select(x => x.ConnectionId).First().Equals(connectionId))
                {
                    _users.Remove(item);
                    return true;
                }
            }
            item.UnregisterConnection(connectionId);
            return false;
        }

        /// <summary>
        /// ���������� ������������� ������������ �� �������������� ����������
        /// </summary>
        /// <param name="connectionId">������������� ����������</param>
        /// <returns><see cref="IConnectedUser"/></returns>
        public IConnectedUser GetUserByConnectionId(string connectionId)
        {
            return GetConnectedUserById(connectionId);
        }

        /// <summary>
        /// ���������� ������������� ������������ �� �������������� ����������
        /// </summary>
        /// <param name="userName">��� ������������ (login)</param>
        /// <returns><see cref="IConnectedUser"/></returns>
        public IConnectedUser GetUserByUserName(string userName)
        {
            return GetConnectedUserByName(userName);
        }

        /// <summary>
        /// ���������� ������ ����������� �� ����
        /// ������� �� ������ ���� ������������� (login)
        /// </summary>
        /// <param name="userNames">������ �������</param>
        /// <param name="type">��� �����������</param>
        /// <returns></returns>
        public IList<string> ConnectionList(IEnumerable<string> userNames, int type)
        {
            if (userNames == null) return null;
            var result = new List<string>();
            var users = _users.Select(x => x.FullName)
                .Where(userName => userNames.Contains(userName, StringComparer.InvariantCultureIgnoreCase));
            foreach (var connections in users.Select(GetConnectedUserByName)
                .Select(userWithConnections => userWithConnections.Connections
                    .Where(x => x.AgentType.Equals(type)).Select(x => x.ConnectionId)))
            {
                result.AddRange(connections);
            }
            return result;
        }

        /// <summary>
        /// ���������� ������������� ������������ �� �������������� �����������
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        private IConnectedUser GetConnectedUserById(string connectionId)
        {
            return _users.FirstOrDefault(x => x.Connections.Select(c => c.ConnectionId).Contains(connectionId));
        }

        /// <summary>
        /// ���������� ������������� ������������ �� ������
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private IConnectedUser GetConnectedUserByName(string userName)
        {
            return _users.FirstOrDefault(x => String.Equals(x.FullName, userName, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}