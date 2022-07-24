using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SRT_UI.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace SRT_UI.Controllers
{
    public class HomeController : Controller
    {
        string strFFMPEGExe = "fef584d5a-dc62-49d4-b5ae-6a46be715e8a";
        string strSRTExe = "sef584d5a-dc62-49d4-b5ae-6a46be715e8a";

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(EncoderConfig myconfig)
        {
            StopServer();
            
            return View(myconfig);
        }
        private void StopServer()
        {
            killFFMpeg();
            killSRTTransmit();
        }

        private void killFFMpeg()
        {
            Console.WriteLine("Killing FFMPEG Process");
            Process[] localByName = Process.GetProcessesByName(strFFMPEGExe);

            foreach (Process p in localByName)
            {
                Console.WriteLine("Found FFMPEG, killing");
                p.Kill();
            }
        }

        private void killSRTTransmit()
        {
            Console.WriteLine("Killing SRT Process");
            Process[] localByName = Process.GetProcessesByName(strSRTExe);

            foreach (Process p in localByName)
            {
                Console.WriteLine("Found SRT, killing");
                p.Kill();
            }
        }

        //public IActionResult Privacy(EncoderConfig myconfig)
        //{
        //    ViewBag.udpIp = myconfig.strUDPIP;
        //    return View();
        //}

        public IActionResult StreamingStarted(EncoderConfig myconfig)
        {
            ViewBag.udpIp = myconfig.strUDPIP;
            ViewBag.udpPort = myconfig.strUDPPort;
            ViewBag.srtIp = myconfig.strSRTIP;
            ViewBag.srtPort = myconfig.strSRTPort;
            ViewBag.mode = myconfig.strSRTMode;
            ViewBag.Vcodec = myconfig.strVideoCodec;
            ViewBag.Vbitrate = myconfig.strVideoBitrate;
            ViewBag.Acodec = myconfig.strAudioCodec;
            ViewBag.Abitrate = myconfig.strAudioBitrate;
            ViewBag.Latenct = myconfig.strSRTLatency;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        public async Task<IActionResult> StartSRTStream(EncoderConfig myConfig)
        {
            if (ModelState.IsValid)
            {
                using (var httpClient = new HttpClient())
                {
                    string strUDPIP = myConfig.strUDPIP;
                    string strUDPPort = myConfig.strUDPPort;
                    string srtIp = myConfig.strSRTIP;
                    string srtPort = myConfig.strSRTPort;
                    string latency = myConfig.strSRTLatency;
                    string strSRTMode = myConfig.strSRTMode;
                    string strVideoCodec = myConfig.strVideoCodec;
                    string strVideoBitrate = myConfig.strVideoBitrate;
                    string strAudioCodec = myConfig.strAudioCodec;
                    string strAudioBitrate = myConfig.strAudioBitrate;

                    var json = JsonConvert.SerializeObject(myConfig);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    using (var response = await httpClient.GetAsync($"https://localhost:7120/api/Home/{strUDPIP}/{strUDPPort}/{srtIp}/{srtPort}/{latency}/{strVideoCodec}/{strVideoBitrate}/{strAudioCodec}/{strAudioBitrate}/{strSRTMode}"))
                    //using (var response = await httpClient.PostAsync("https://localhost:7120/api/Home", data))
                    {
                        //apiResponse = await response.Content.ReadAsStringAsync();
                        //srtModel = JsonConvert.DeserializeObject<EncoderConfig>(apiResponse);

                    }
                }

                //return RedirectToAction("Completed", "SRT");
                return RedirectToAction("StreamingStarted", myConfig);
            }

            return RedirectToAction("Index", myConfig);
            //return Ok();
        }
    }
}