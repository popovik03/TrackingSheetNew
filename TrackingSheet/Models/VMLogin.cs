using System.ComponentModel.DataAnnotations;

namespace TrackingSheet.Models
{
    public class VMLogin
    {
        [Required(ErrorMessage = "Не указан 431")]
        public string Login431 { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string PassWord { get; set; }

        public bool KeepLogged { get; set; }
    }

}
