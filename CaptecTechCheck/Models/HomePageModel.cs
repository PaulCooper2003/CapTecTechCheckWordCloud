using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CaptecTechCheck.Models
{
    public class HomePageModel
    {
        public HomePageModel()
        {
            cloudModel = new List<CloudModel>();
        }
        [Required]
        public string URL { get; set; }        

        public List<CloudModel> cloudModel { get; set; }
    }
}