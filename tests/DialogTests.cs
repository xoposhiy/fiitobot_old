using FakeItEasy;
using fiitobot;
using fiitobot.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace tests;

public class DialogTests
{
    private ContactsRepository? contactsRepo;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        contactsRepo = new ContactsRepoBuilder().Build();
    }
    
    [TestCase("Мизурова")]
    [TestCase("Дарья Мизурова")]
    [TestCase("Мизурова Дарья")]
    [TestCase("udarenienao")]
    [TestCase("@udarenienao")]
    public void PlainTextMessage_SearchesStudent(string query)
    {
        var contactsPresenter = A.Fake<IPresenter>();
        var handleUpdateService = PrepareUpdateService(contactsPresenter);
        
        handleUpdateService.HandlePlainText(query, 123);
        
        A.CallTo(() => contactsPresenter.ShowContact(
                A<Contact>.That.Matches(c => c.LastName == "Мизурова"), 
                123))
            .MustHaveHappenedOnceExactly();
    }
    
    [TestCase("Мизурова Дарья Неизвестнокакоевна")]
    public void PlainTextMessage_SearchesStudent_IgnoringSomeParts(string query)
    {
        var contactsPresenter = A.Fake<IPresenter>();
        var handleUpdateService = PrepareUpdateService(contactsPresenter);
        
        handleUpdateService.HandlePlainText(query, 123);
        
        A.CallTo(() => contactsPresenter.ShowContact(
                A<Contact>.That.Matches(c => c.LastName == "Мизурова"), 
                123))
            .MustHaveHappenedOnceExactly();
    }
    
    [TestCase("Иван")]
    public void PlainTextMessage_SearchesManyStudents(string firstName)
    {
        var contactsPresenter = A.Fake<IPresenter>();
        var handleUpdateService = PrepareUpdateService(contactsPresenter);
        
        handleUpdateService.HandlePlainText(firstName, 42);
        
        A.CallTo(() => contactsPresenter.ShowContact(
                A<Contact>.That.Matches(c => c.FirstName == firstName), 
                42))
            .MustHaveHappenedTwiceOrMore();
    }
    
    private HandleUpdateService PrepareUpdateService(IPresenter presenter)
    {
        return new HandleUpdateService(NullLogger<HandleUpdateService>.Instance, contactsRepo!, presenter);
    }
}