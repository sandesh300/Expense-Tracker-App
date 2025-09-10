using Expense_Tracker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var sendGridSettings = new SendGridSettings();
builder.Configuration.GetSection("SendGrid").Bind(sendGridSettings);

//if (string.IsNullOrEmpty(sendGridSettings.ApiKey))
//{
//    // This will cause the application to fail to start if the API key is not found,
//    // which is exactly what we want. It immediately tells you the configuration is wrong.
//    throw new InvalidOperationException("SendGrid API Key is not configured in User Secrets.");
//}
builder.Services.AddSingleton(sendGridSettings); // Add as a singleton service
builder.Services.AddTransient<IEmailService, SmtpEmailService>();


//dependency injection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();


//Register Syncfusion license
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2UFhhQlJBfVpdVHxLflFyVWFTe156dVZWESFaRnZdRl1nSXdTdEFlWXZWeXJc\r\n");


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
