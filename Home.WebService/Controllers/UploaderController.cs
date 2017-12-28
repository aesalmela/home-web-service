using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Home.WebService.Tools;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Home.WebService.Controllers
{
    [Route("[controller]")]
    public class UploaderController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IOptions<UploadSettings> _uploadSettings;

        public UploaderController(IOptions<AppSettings> appSettings, IOptions<UploadSettings> uploadSettings)
        {
            _appSettings = appSettings;
            _uploadSettings = uploadSettings;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new StringContent(ex.Message));
            }
            finally { }
        }

        [HttpPost]
        public IActionResult Post()
        {
            IActionResult result = StatusCode(500, new StringContent("An unknown error occurred."));
            try
            {
                string serverSS = _appSettings.Value.SharedSecret;
                string picDest = _uploadSettings.Value.PicLocation;

                if (Directory.Exists(picDest))
                {
                    if (Request.HasFormContentType)
                    {
                        var form = Request.Form;
                        string clientSS = AESEncryption.Decrypt(Request.Form["secret"]);
                        if (serverSS == clientSS)
                        {
                            foreach (var formFile in form.Files)
                            {
                                var fileName = formFile.FileName;
                                var savePath = Path.Combine(picDest, fileName);
                                using (var fileStream = new FileStream(savePath, FileMode.Create))
                                {
                                    formFile.CopyTo(fileStream);
                                }
                            }
                            result = Ok();
                        }
                        else
                        {
                            result = Unauthorized();
                        }
                    }
                    else
                    {
                        result = BadRequest();
                    }
                }
                else
                {
                    result = StatusCode(500, new StringContent("The destination directory does not exist."));
                }
            }

            catch (Exception ex)
            {
                result = StatusCode(500, new StringContent(ex.Message));
            }
            finally { }

            return result;
        }


    }
}