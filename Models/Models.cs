using System.ComponentModel.DataAnnotations;
namespace RepairLab.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RepairCase
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Введите название"), StringLength(120)]
    [Display(Name = "Название")] public string Title { get; set; }
    [Required, Display(Name = "Устройство")] public string DeviceType { get; set; }
    [Required(ErrorMessage = "Опишите дефект"), StringLength(200)]
    [Display(Name = "Дефект")] public string Defect { get; set; }
    [Required, Display(Name = "Метод")] public string Method { get; set; }
    [Required, Display(Name = "Масштаб")] public string Scale { get; set; }
    [Required(ErrorMessage = "Добавьте описание"), StringLength(2000)]
    [Display(Name = "Описание")] public string Description { get; set; }
    public string Status { get; set; }
    public int UserId { get; set; }
    public string AuthorName { get; set; }
    public DateTime CreatedAt { get; set; }

    public static readonly string[] Devices = { "Ноутбук", "ПК / материнская плата", "Видеокарта", "Смартфон", "Другое" };
    public static readonly string[] Methods = { "SEM-диагностика", "FIB-срез и локализация дефекта", "FIB-восстановление дорожки (напыление Pt/W)", "Раскрытие и расплетение металлизации слоёв" };
    public static readonly string[] Scales = { "нм", "мкм" };
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите имя"), Display(Name = "Имя")] public string FullName { get; set; }
    [Required, EmailAddress(ErrorMessage = "Неверный email")] public string Email { get; set; }
    [Required, MinLength(6, ErrorMessage = "Минимум 6 символов"), DataType(DataType.Password), Display(Name = "Пароль")] public string Password { get; set; }
    [Required, Compare("Password", ErrorMessage = "Пароли не совпадают"), DataType(DataType.Password), Display(Name = "Подтверждение пароля")] public string ConfirmPassword { get; set; }
}

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; }
    [Required, DataType(DataType.Password), Display(Name = "Пароль")] public string Password { get; set; }
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Введите имя"), Display(Name = "Имя")] public string FullName { get; set; }
    [Required, EmailAddress(ErrorMessage = "Неверный email")] public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password), Display(Name = "Старый пароль")] public string OldPassword { get; set; }
    [Required, MinLength(6, ErrorMessage = "Минимум 6 символов"), DataType(DataType.Password), Display(Name = "Новый пароль")] public string NewPassword { get; set; }
    [Required, Compare("NewPassword", ErrorMessage = "Пароли не совпадают"), DataType(DataType.Password), Display(Name = "Подтверждение")] public string ConfirmPassword { get; set; }
}

public class ContactViewModel
{
    [Required(ErrorMessage = "Введите имя"), Display(Name = "Имя")] public string Name { get; set; }
    [Required, EmailAddress(ErrorMessage = "Неверный email")] public string Email { get; set; }
    [Required(ErrorMessage = "Введите сообщение"), StringLength(1000), Display(Name = "Сообщение")] public string Message { get; set; }
}
