using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace comino_print_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services = ConfigureServices().services
            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddCors()
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}