-- فارسی‌سازی گزینه‌های dropdown

UPDATE AppInputFields SET Options = N'["واقع‌گرایانه","هنر دیجیتال","نقاشی رنگ روغن","آبرنگ","انیمه","سینمایی","مینیمالیست","سوررئالیسم"]'
WHERE Name = 'art_style';

UPDATE AppInputFields SET Options = N'["حماسی","آرام","تاریک","شاد","مرموز","رمانتیک","غمگین"]'
WHERE Name = 'mood';

UPDATE AppInputFields SET Options = N'["پایتون (Python)","جاوا اسکریپت (JavaScript)","تایپ اسکریپت (TypeScript)","سی‌شارپ (C#)","جاوا (Java)","گو (Go)","راست (Rust)","پی‌اچ‌پی (PHP)","سویفت (Swift)","کاتلین (Kotlin)"]'
WHERE Name = 'language';

SELECT Name, Options FROM AppInputFields WHERE Name IN ('art_style','mood','language');
