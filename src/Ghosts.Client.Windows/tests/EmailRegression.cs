using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

class EmailRegression
{
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    static void Main()
    {
        var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var client = Assembly.LoadFrom(Path.Combine(directory, "ghosts.exe"));
        var domain = Assembly.LoadFrom(Path.Combine(directory, "Ghosts.Domain.dll"));
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var config = Activator.CreateInstance(domain.GetType("Ghosts.Domain.Code.ClientConfiguration"));
        config.GetType().GetProperty("EmailContent").SetValue(config,
            new Dictionary<string, string> { { "person", "James Smith" } }, null);
        var type = client.GetType("Ghosts.Client.Infrastructure.Email.EmailContentManager");
        var manager = FormatterServices.GetUninitializedObject(type);
        type.GetProperty("Configuration").SetValue(manager, config, null);
        var parse = type.GetMethod("Parse", flags);
        var body = @"Please review the firewall rules.\nKeep the approved exceptions.\nThanks, <person/>";
        var expected = "Please review the firewall rules." + Environment.NewLine +
            "Keep the approved exceptions." + Environment.NewLine + "Thanks, James Smith";
        Check((string)parse.Invoke(manager, new object[] { body }) == expected, "Authored body, tokens or newlines lost");
        body = "Subject review is due tomorrow.";
        Check((string)parse.Invoke(manager, new object[] { body }) == body, "Authored subject text stripped");
        body = @"From: Alice\nSent: Monday\nTo: Bob\nSubject: Review Please check the rules.";
        Check(((string)parse.Invoke(manager, new object[] { body })).Contains("Please check the rules"), "Legacy body lost");
        type = client.GetType("Ghosts.Client.Infrastructure.Email.EmailConfiguration");
        var email = FormatterServices.GetUninitializedObject(type);
        var from = type.GetField("<From>k__BackingField", flags);
        var footer = type.GetMethod("GetFooter", flags);
        from.SetValue(email, "CurrentUser");
        Check(!((string)footer.Invoke(email, null)).Contains("CurrentUser"), "CurrentUser leaked into footer");
        from.SetValue(email, "sender@example.org");
        Check(((string)footer.Invoke(email, null)).Contains("sender@example.org"), "Explicit sender replaced");
        Console.WriteLine("PASS: authored bodies, tokens/newlines, legacy body, CurrentUser and explicit sender footers");
    }
}
