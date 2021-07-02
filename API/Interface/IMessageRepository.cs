using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interface
{
    public interface IMessageRepository
    {
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection> GetConnection(string connectionId);
        Task<Group> GetMessageGroup(string groupName);
        void AddMessage(Message message);

        Task<Group> GetGroupForConnection(string connectionId);

        void DeleteMessage(Message message);

        Task<Message> GetMessage(int id);

        Task<PageList<MessageDto>> GetMessageForUser(MessageParams messageParams);
        
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipietUsername);
        

    }
}