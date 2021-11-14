using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json;
using System.Media;

namespace WinFormsApp1
{
    public partial class Form2 : Form
    {
        class ZaloResponse
        {
            public int error_code { get; set; }
            public string error_message { get; set; }
            public AudioURL data { get; set; }
        }

        class AudioURL
        {
            public string url { get; set; }
        }

        public Form2()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string file = openFileDialog1.FileName;
                try
                {
                    string text = File.ReadAllText(file);
                    int size = text.Length;
                }
                catch (IOException)
                {
                    throw;
                }
            }
            _bookPath = openFileDialog1.FileName;
            var ExtractedPDFToString = ExtractTextFromPdf(_bookPath);
            textBox1.Text = ExtractedPDFToString;
        }

        private string ExtractTextFromPdf(string path)
        {
            using PdfReader reader = new(path);
            StringBuilder text = new();

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
            }

            return text.ToString();
        }

        async Task CallApiAsync(string content)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.zalo.ai/v1/tts/synthesize");
            request.Headers.TryAddWithoutValidation("apikey", "X8DEEy2VxgisgSxJa9xHcIoec4n5M3gi");

            var contentList = new List<string>
                    {
                        $"input={Uri.EscapeDataString(content)}",
                        "speaker_id=1",
                        "encode_type=0"
                    };
            request.Content = new StringContent(string.Join("&", contentList));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            var response = await httpClient.SendAsync(request);
            var res = response.Content.ReadAsStringAsync().Result;

            var zaloResponse = JsonConvert.DeserializeObject<ZaloResponse>(res);

            label2.Text = zaloResponse.data.url;

            _audioURL = zaloResponse.data.url;

            
        }

        public string _audioURL;
        public string _audioPath;
        public string _bookPath;

        void DownloadAudio()
        {
            using (var client = new WebClient())
            {
                _audioPath = _audioURL.Substring(47, 20) + ".mp3";
                client.DownloadFile(_audioURL, _audioPath);
            }
        }

        private async void Button2_Click(object sender, EventArgs e)
        {
            var content = textBox1.Text.Substring(0, 100);
            await CallApiAsync(content);
            DownloadAudio();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            PlayAudio();
        }

        void PlayAudio()
        {
            SoundPlayer soundPlayer = new();
            try
            {
                soundPlayer.SoundLocation = _audioPath;
                soundPlayer.PlaySync();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
