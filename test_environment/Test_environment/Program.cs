using Ganss.Xss;

const string HTML = """<script> b = document.getElementById("a"); b.addEventListener('hover', console.log("evil things")); </script> <div><p id="a"> Harmless-looking text </p><p> <a href="duckduckgo.com"> Harmless link </p></div>""";

var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedAttributes.Add("alt");
        sanitizer.AllowedAttributes.Add("src");
        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedTags.Add("i");
        sanitizer.AllowedTags.Add("a");
        sanitizer.AllowedTags.Add("b");
        sanitizer.AllowedTags.Add("img");
        Console.Write(sanitizer.Sanitize(HTML));