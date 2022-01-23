using fiitobot;
using NUnit.Framework;

namespace tests;

public class ContactsRepositoryTests
{
    [Test]
    public void GetContacts()
    {
        var repo = new ContactsRepoBuilder().Build();
        var contacts = repo.FindContacts("Мизурова");
        var dasha = new Contact(AdmissionYear: 2020, LastName: "Мизурова",
            FirstName: "Дарья", Patronymic: "", GroupIndex: 2, SubgroupIndex: 2, City: "Екатеринбург",
            School: "СУНЦ УрФУ", Concurs: "О", Rating: "276", Telegram: "@udarenienao", Phone: "", Email: "",
            Note: "");
        Assert.That(contacts, Is.EqualTo(new[]{dasha}));
    }
}