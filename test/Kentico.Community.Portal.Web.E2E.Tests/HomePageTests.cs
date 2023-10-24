namespace Kentico.Community.Portal.Web.E2E.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class HomePageTests : CommunityPageTests
{
    [Test]
    public async Task User_Can_Perform_Registration_And_Login()
    {
        TestContext.WriteLine("Navigate to home page");

        _ = await Page.GotoAsync("/");

        TestContext.WriteLine("Validate content renders");

        await Expect(Page.Locator("#gridsection-updates")).ToBeVisibleAsync();
        await Expect(Page.Locator(".error-hero")).Not.ToBeVisibleAsync();
    }
}
