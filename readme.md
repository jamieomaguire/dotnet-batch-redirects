# .NET Batch Redirects
This is a tool I wrote to automate adding redirects to ASP.NET MVC projects I've been working on.

Redirects are usually provided as a CSV file. They are then added to the project via the IIS UrlRewrite module. As they are done manually, one at a time this can be rather time consuming and is prone to human error.

I've built this console application to process those redirects and add them to the rewritemap within the project's web.config file, all at once.