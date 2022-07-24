using System.ComponentModel.DataAnnotations;

namespace SRT_UI.Models
{
    public class EncoderConfig
    {
        //public string strSelectedCard { get; set; }
        //public string strInputType { get; set; }

        [Required(ErrorMessage = "Bu Alanın Doldurulması Mecburidir.")]
        public string strUDPIP { get; set; }
        [Required(ErrorMessage = "Bu Alanın Doldurulması Mecburidir.")]
        public string strUDPPort { get; set; }
        [Required(ErrorMessage = "Bu Alanın Doldurulması Mecburidir.")]
        public string strSRTIP { get; set; }
        [Required(ErrorMessage = "Bu Alanın Doldurulması Mecburidir.")]
        public string strSRTPort { get; set; }
        [Required(ErrorMessage = "Bu Alanın Doldurulması Mecburidir.")]
        public string strSRTLatency { get; set; }
        public string strSRTMode { get; set; }
        public string strVideoCodec { get; set; }
        public string strVideoBitrate { get; set; }
        public string strAudioCodec { get; set; }
        public string strAudioBitrate { get; set; }
        //public string strSRTPass { get; set; }
    }
}
