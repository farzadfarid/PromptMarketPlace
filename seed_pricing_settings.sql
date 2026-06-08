-- ══════════════════════════════════════════
-- تنظیمات قیمت‌گذاری اعتبار
-- ══════════════════════════════════════════

INSERT INTO SystemSettings ([Key],[Value],[Group],[Description],IsEncrypted) VALUES
('Pricing:UsdToIrrRate',    '900000',  'Pricing', N'نرخ تبدیل دلار به ریال (مثال: 900000)', 0),
('Pricing:VatPercent',      '9',       'Pricing', N'مالیات بر ارزش افزوده به درصد (طبق قانون ایران ۹٪)', 0),
('Pricing:MarginPercent',   '30',      'Pricing', N'سود خالص پلتفرم به درصد', 0),
('Pricing:CreditValueIrr',  '1000',    'Pricing', N'ارزش هر ۱ اعتبار به ریال', 0),
('Pricing:AvgTextTokens',   '1500',    'Pricing', N'میانگین توکن مصرفی در اجرای ابزار متنی', 0),
('Pricing:AvgVideoSeconds', '60',      'Pricing', N'میانگین ثانیه ویدیو در هر اجرا', 0),
-- قیمت نهایی هر نوع خروجی (اعتبار) — ادمین می‌تواند override کند
('Pricing:TextCreditCost',  '1',       'Pricing', N'اعتبار لازم برای اجرای ابزار متن/کد/فرم', 0),
('Pricing:ImageCreditCost', '5',       'Pricing', N'اعتبار لازم برای اجرای ابزار تصویر', 0),
('Pricing:VideoCreditCost', '20',      'Pricing', N'اعتبار لازم برای اجرای ابزار ویدیو', 0),
('Pricing:AudioCreditCost', '3',       'Pricing', N'اعتبار لازم برای اجرای ابزار صدا', 0);

SELECT [Key],[Value],[Description] FROM SystemSettings WHERE [Group]='Pricing' ORDER BY [Key];
