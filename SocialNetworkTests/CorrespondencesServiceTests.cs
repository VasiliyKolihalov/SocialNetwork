using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Xunit;
using Moq;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Correspondences;
using SocialNetwork.Models.Messages;
using SocialNetwork.Models.Users;
using SocialNetwork.Repositories;
using SocialNetwork.Repositories.Correspondences;
using SocialNetwork.Repositories.Messages;
using SocialNetwork.Repository;
using SocialNetwork.Services;

namespace SocialNetworkTests;

public class CorrespondencesServiceTests
{
    [Fact]
    public void GetAll_ShouldReturnAllCorrespondences()
    {
        IEnumerable<Correspondence> testCorrespondences = GetTestCorrespondences();

        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Correspondences.GetUserCorrespondences(It.IsAny<int>()))
            .Returns(testCorrespondences);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        IEnumerable<CorrespondencePreviewModel> result = correspondencesService.GetAll(It.IsAny<int>());

        Assert.NotNull(result);
        Assert.Equal(testCorrespondences.Count(), result.Count());
    }

    [Fact]
    public void GetWithMessages_ShouldReturnCorrespondenceWithMessages()
    {
        User user = GetTestUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Users.Add(user);
        IEnumerable<Message> testMessages = GetTestMessages();

        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>()))
            .Returns(testCorrespondence);
        applicationContextMock.Setup(x => x.Messages.GetFromCorrespondence(It.IsAny<int>()))
            .Returns(testMessages);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        CorrespondenceViewModel result = correspondencesService.GetWithMessages(It.IsAny<int>(), user.Id);
        
        Assert.NotNull(result);
        Assert.Equal(testCorrespondence.Users.Count ,result.Users.Count);
        Assert.Equal(testMessages.Count(), result.Messages.Count);
    }
    
    [Fact]
    public void GetWithMessages_ShouldThrowBecauseNotFound()
    {
        Correspondence testCorrespondence = GetTestCorrespondence();

        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>()))
            .Returns(testCorrespondence);
       
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        Action resultAction = () => correspondencesService.GetWithMessages(It.IsAny<int>(), It.IsAny<int>());
        
        Assert.Throws<NotFoundException>(resultAction);
    }

    [Fact]
    public void StartCorrespondence_ShouldReturnNewCorrespondence()
    {
        User testSenderUser = GetTestSenderUser();
        IEnumerable<User> testUsers = GetTestUsers();
        CorrespondenceAddModel correspondenceAddModel = new CorrespondenceAddModel()
        {
            MessageAddModel = GetTestMessageAddModel(),
            Name = "Correspondence1",
            ParticipantsId = testUsers.Select(x => x.Id).ToList()
        };
        
        var applicationContextMock = new Mock<IApplicationContext>();
        foreach (var testUser in testUsers)
        {
            applicationContextMock.Setup(x => x.Users.Get(testUser.Id))
                .Returns(testUser);
        }

        applicationContextMock.Setup(x => x.Correspondences).Returns(Mock.Of<ICorrespondencesRepository>());
        applicationContextMock.Setup(x => x.Messages).Returns(Mock.Of<IMessagesRepository>());

        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        CorrespondencePreviewModel result = correspondencesService.StartCorrespondence(correspondenceAddModel, testSenderUser.Id);
        
        Assert.NotNull(result);
        Assert.Equal(testUsers.Count() + 1, result.Users.Count);
        Assert.Equal(correspondenceAddModel.Name, result.Name);
    }

    [Fact]
    public void StartCorrespondence_ShouldThrowBecauseUserRepeat()
    {
        IEnumerable<User> testUsers = GetTestUsers();
        CorrespondenceAddModel correspondenceAddModel = new CorrespondenceAddModel()
        {
            Name = "Correspondence1",
            ParticipantsId = testUsers.Select(x => x.Id).ToList()
        };
        correspondenceAddModel.ParticipantsId.Add(testUsers.First().Id);

        var applicationContextMock = new Mock<IApplicationContext>();
        foreach (var testUser in testUsers)
        {
            applicationContextMock.Setup(x => x.Users.Get(testUser.Id))
                .Returns(testUser);
        }

        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        Action resultAction = () => correspondencesService.StartCorrespondence(correspondenceAddModel, It.IsAny<int>());

        Assert.Throws<BadRequestException>(resultAction);
    }

    [Fact]
    public void AddUserToCorrespondence_ShouldReturnCorrespondence()
    {
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Admin = testSenderUser;
        User testUser = GetTestUniqueUser();
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Users.Get(It.IsAny<int>())).Returns(It.IsAny<User>());
        applicationContextMock.Setup(x => x.Correspondences).Returns(Mock.Of<ICorrespondencesRepository>());
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        CorrespondencePreviewModel result = correspondencesService.AddUserToCorrespondence(testCorrespondence.Id, testUser.Id, testSenderUser.Id);
        
        Assert.NotNull(result);
        Assert.Equal(testCorrespondence.Users.Count ,result.Users.Count);
    }

    [Fact]
    public void AddUserToCorrespondence_ShouldThrowBecauseNotEnoughRights()
    {
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Admin = GetTestUniqueUser();
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Users.Get(It.IsAny<int>())).Returns(It.IsAny<User>());
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);

        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        Action resultAction = () =>correspondencesService.AddUserToCorrespondence(testCorrespondence.Id, It.IsAny<int>(), testSenderUser.Id);

        Assert.Throws<BadRequestException>(resultAction);
    }
    
    [Fact]
    public void AddUserToCorrespondence_ShouldThrowBecauseUserIsAlreadyInCorrespondence()
    {
        User testUser = GetTestUniqueUser();
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Admin = testSenderUser;
        testCorrespondence.Users.Add(testUser);
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Users.Get(It.IsAny<int>())).Returns(It.IsAny<User>());
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);

        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        Action resultAction = () =>correspondencesService.AddUserToCorrespondence(testCorrespondence.Id, testUser.Id, testSenderUser.Id);

        Assert.Throws<BadRequestException>(resultAction);
    }

    [Fact]
    public void DeleteUserFromCorrespondence_ShouldReturnCorrespondence()
    {
        User testSenderUser = GetTestSenderUser();
        User testUser = GetTestUniqueUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Admin = testSenderUser;
        testCorrespondence.Users.Add(testUser);

        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Users.Get(It.IsAny<int>())).Returns(It.IsAny<User>());
        applicationContextMock.Setup(x => x.Correspondences).Returns(Mock.Of<ICorrespondencesRepository>());
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        CorrespondencePreviewModel result = correspondencesService.DeleteUserFromCorrespondence(testCorrespondence.Id, testUser.Id, testSenderUser.Id);
        
        Assert.NotNull(result);
        Assert.Equal(testCorrespondence.Users.Count ,result.Users.Count);
    }
    
    [Fact]
    public void DeleteUserFromCorrespondence_ShouldThrowBecauseNotEnoughRights()
    {
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Admin = GetTestUniqueUser();

        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Users.Get(It.IsAny<int>())).Returns(It.IsAny<User>());
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);

        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        Action resultAction = () =>correspondencesService.DeleteUserFromCorrespondence(testCorrespondence.Id, It.IsAny<int>(), testSenderUser.Id);

        Assert.Throws<BadRequestException>(resultAction);
    }

    [Fact]
    public void DeleteUserFromCorrespondence_ShouldBecauseUserNotInCorrespondence()
    {
        User testUser = GetTestUniqueUser();
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Admin = testSenderUser;
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Users.Get(It.IsAny<int>())).Returns(It.IsAny<User>());
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);

        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        Action resultAction = () =>correspondencesService.DeleteUserFromCorrespondence(testCorrespondence.Id, testUser.Id, testSenderUser.Id);

        Assert.Throws<BadRequestException>(resultAction);
    }

    [Fact]
    public void Edit_ShouldReturnCorrespondence()
    {
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Admin = testSenderUser;

        CorrespondenceEditModel correspondenceEditModel = new CorrespondenceEditModel()
            {Id = testCorrespondence.Id, Name = "NewCorrespondence1Name"};
            
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Correspondences).Returns(Mock.Of<ICorrespondencesRepository>());
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        CorrespondencePreviewModel result = correspondencesService.Edit(correspondenceEditModel, testSenderUser.Id);
        
        Assert.NotNull(result);
        Assert.Equal(testCorrespondence.Id, result.Id);
        Assert.NotEqual(testCorrespondence.Name, result.Name);
    }

    [Fact]
    public void Edit_ShouldThrowBecauseNotEnoughRights()
    {
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Admin = GetTestUniqueUser();

        CorrespondenceEditModel correspondenceEditModel = new CorrespondenceEditModel()
            {Id = testCorrespondence.Id};
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        Action result = () => correspondencesService.Edit(correspondenceEditModel, testSenderUser.Id);

        Assert.Throws<BadRequestException>(result);
    }

    [Fact]
    public void Delete_ShouldReturnCorrespondence()
    {
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Admin = testSenderUser;
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);

        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());
        
        CorrespondencePreviewModel result = correspondencesService.Delete(testCorrespondence.Id, testSenderUser.Id);
        
        Assert.NotNull(result);
        Assert.Equal(testCorrespondence.Users.Count ,result.Users.Count);
    }

    [Fact]
    public void Delete_ShouldThrowBecauseNotEnoughRights()
    {
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Admin = GetTestUniqueUser();
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);

        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());
        
        Action result = () => correspondencesService.Delete(testCorrespondence.Id, testSenderUser.Id);
        
        Assert.Throws<BadRequestException>(result);
    }

    [Fact]
    public void SendMessage_ShouldReturnCorrespondence()
    {
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondence();
        testCorrespondence.Users.Add(testSenderUser);
        MessageAddModel messageAddModel = GetTestMessageAddModel();
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);
        applicationContextMock.Setup(x => x.Users.Get(It.IsAny<int>())).Returns(testSenderUser);
        applicationContextMock.Setup(x => x.Messages).Returns(Mock.Of<IMessagesRepository>());
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        MessageViewModel result = correspondencesService.SendMessage(messageAddModel, testCorrespondence.Id, testSenderUser.Id);
        
        Assert.NotNull(result);
        Assert.Equal(messageAddModel.Text.Length, result.Text.Length);
    } 
    
    [Fact]
    public void SendMessage_ShouldThrowBecauseUserNotInCorrespondence()
    {
        User testSenderUser = GetTestSenderUser();
        Correspondence testCorrespondence = GetTestCorrespondences().First();
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Correspondences.Get(It.IsAny<int>())).Returns(testCorrespondence);
        applicationContextMock.Setup(x => x.Users.Get(It.IsAny<int>())).Returns(testSenderUser);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        Action result = () =>correspondencesService.SendMessage(It.IsAny<MessageAddModel>(), testCorrespondence.Id, testSenderUser.Id);
        
        Assert.Throws<NotFoundException>(result);
    }

    [Fact]
    public void EditMessage_ShouldReturnCorrespondence()
    {
        User testSenderUser = GetTestSenderUser();
        Message testMessage = GetTestMessage();
        testMessage.Sender = testSenderUser;
        MessageEditModel messageEditModel = new MessageEditModel() {Id = testMessage.Id, Text = "UpdatedMessage"};

        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Messages).Returns(Mock.Of<IMessagesRepository>());
        applicationContextMock.Setup(x => x.Messages.Get(It.IsAny<long>())).Returns(testMessage);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());
        
        MessageViewModel result = correspondencesService.EditMessage(messageEditModel, testSenderUser.Id);
        
        Assert.NotNull(result);
        Assert.Equal(testMessage.Id, result.Id);
        Assert.True(result.IsEdited);
        Assert.NotEqual(testMessage.Text, result.Text);
    }
    
    [Fact]
    public void EditMessage_ShouldThrowBecauseMessageNotBelongToUser()
    {
        User testSenderUser = GetTestSenderUser();
        Message testMessage = GetTestMessage();
        testMessage.Sender = GetTestUniqueUser();
        MessageEditModel messageEditModel = new MessageEditModel() {Id = testMessage.Id, Text = "UpdatedMessage"};

        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Messages.Get(It.IsAny<long>())).Returns(testMessage);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());
        
        Action result = () => correspondencesService.EditMessage(messageEditModel, testSenderUser.Id);
        
        Assert.Throws<NotFoundException>(result);
    }

    [Fact]
    public void DeleteMessage_ShouldReturnCorrespondence()
    {        
        User testSenderUser = GetTestSenderUser();
        Message testMessage = GetTestMessage();
        testMessage.Sender = testSenderUser;
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Messages).Returns(Mock.Of<IMessagesRepository>());
        applicationContextMock.Setup(x => x.Messages.Get(It.IsAny<long>())).Returns(testMessage);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        MessageViewModel result = correspondencesService.DeleteMessage(testMessage.Id, testSenderUser.Id);
        
        Assert.NotNull(result);
    }
    
    [Fact]
    public void DeleteMessage_ShouldThrowBecauseMessageNotBelongToUser()
    {
        User testSenderUser = GetTestSenderUser();
        Message testMessage = GetTestMessage();
        testMessage.Sender = GetTestUniqueUser();
        
        var applicationContextMock = new Mock<IApplicationContext>();
        applicationContextMock.Setup(x => x.Messages.Get(It.IsAny<long>())).Returns(testMessage);
        
        var correspondencesService = new CorrespondencesService(applicationContextMock.Object, GetMapper());

        Action result = () => correspondencesService.DeleteMessage(testMessage.Id, testSenderUser.Id);
        
        Assert.Throws<NotFoundException>(result);
    }
    
    private static IEnumerable<Correspondence> GetTestCorrespondences()
    {
        IEnumerable<Correspondence> correspondences = new[]
        {
            new Correspondence() {Id = 1, Name = "Correspondence1", Users = GetTestUsers().ToList()},
            new Correspondence() {Id = 2, Name = "Correspondence2", Users = GetTestUsers().ToList()},
            new Correspondence() {Id = 3, Name = "Correspondence3", Users = GetTestUsers().ToList()}
        };
        return correspondences;
    }

    private static Correspondence GetTestCorrespondence()
    {
        return GetTestCorrespondences().First();
    }

    private static IEnumerable<Message> GetTestMessages()
    {
        IEnumerable<Message> messages = new[]
        {
            new Message() {Id = 1, Text = "Message1"},
            new Message() {Id = 2, Text = "Message2"},
            new Message() {Id = 3, Text = "Message3"}
        };
        return messages;
    }

    private static Message GetTestMessage()
    {
        return GetTestMessages().First();
    }

    private static IEnumerable<User> GetTestUsers()
    {
        IEnumerable<User> users = new[]
        {
            new User() {Id = 1, FirstName = "User1"},
            new User() {Id = 2, FirstName = "User2"},
            new User() {Id = 3, FirstName = "User3"}
        };
        return users;
    }

    private static User GetTestUser()
    {
        return GetTestUsers().First();
    }

    private static User GetTestSenderUser()
    {
        return new User() {Id = 0, FirstName = "User0"};
    }

    private static User GetTestUniqueUser()
    {
        return new User() {Id = -1, FirstName = "User-1"};
    }

    private static MessageAddModel GetTestMessageAddModel()
    {
        return new MessageAddModel() {Text = "Message1"};
    }
    
    private static IMapper GetMapper()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<Correspondence, CorrespondenceViewModel>();
            cfg.CreateMap<CorrespondenceAddModel, Correspondence>();
            cfg.CreateMap<CorrespondenceEditModel, Correspondence>();
            
            cfg.CreateMap<Message, MessageViewModel>();
            cfg.CreateMap<MessageAddModel, Message>();
            cfg.CreateMap<MessageEditModel, Message>();

            cfg.CreateMap<User, UserPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        return mapper;
    }
}