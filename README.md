# fiitobot

Для работы tg-bot нужны следующие переменные:

* `BotToken` — токен телеграм-бота, полученный у BotFather
* `HostAddress` — публичный адрес, на котором разворачивается это приложение и который регистрируется в качестве веб-хука.
* `ContactsSpreadsheetId` — идентификатор гугл-таблицы, где все контакты.
* `GOOGLE_AUTH_JSON` — содержимое googl-credentials.json, для сервис-аккаунта от имени которого бот будет ходить по гуглтаблицам.

Все эти переменные можно задать переменными среды, либо в файле appsettings.json. 

Для разработки удобнее всего создать appsettings.Development.json файл такого содержания:

```json
{
  "BotToken": "...",
  "HostAddress": "...",
  "ContactsSpreadsheetId": "...",
  "GOOGLE_AUTH_JSON": "..."
}
```
