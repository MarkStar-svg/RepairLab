# RepairLab — ремонт плат с FIB-SEM (ASP.NET Core MVC + ADO.NET)

## Запуск
1. Нужны .NET 8 SDK и SQL Server LocalDB (идёт с Visual Studio) либо любой SQL Server — поправьте строку подключения в `appsettings.json`.
2. `dotnet run`
3. База `RepairLab`, таблицы и демо-данные создаются автоматически при первом старте.

Админ: `admin@repairlab.local` / `Admin123`. Обычные пользователи регистрируются сами.

## Соответствие чек-листу
- Layout + меню: `Views/Shared/_Layout.cshtml`; 404: `UseStatusCodePagesWithReExecute` -> `Home/Error404`
- Partial Views: `_Card`, `_CaseForm`, `_CabinetNav`
- ADO.NET (SqlConnection/SqlCommand/SqlDataReader): `Data/*Repository.cs`
- Роли User/Admin, cookie-аутентификация, PBKDF2-хеширование паролей
- Изоляция данных ЛК: `WHERE Id=@id AND UserId=@u` (`CaseRepository.GetOwned/Update/Delete`)
- Валидация: Data Annotations, уникальный email, пароль от 6 символов, подтверждение пароля
- Адаптивность от 375px: `wwwroot/css/site.css`

## Страницы
/ , /Items , /Items/Details/{id} , /Home/About , /Home/Contacts , /Account/Login , /Account/Register ,
/Cabinet , /Cabinet/MyItems , /Cabinet/Create , /Cabinet/Edit/{id} , /Cabinet/Delete/{id} , /Cabinet/Profile , /Cabinet/ChangePassword
