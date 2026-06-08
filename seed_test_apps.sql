-- ═══════════════════════════════════════════════════════
-- SEED: Test Apps for Farzad (Creator)
-- UserId: fe5dd038-9621-4675-a8e2-bd8e44123886
-- OutputTypes: Text(0) Image(1) Video(2) Form(3) Code(4) Audio(5)
-- ═══════════════════════════════════════════════════════

BEGIN TRANSACTION;

DECLARE @CreatorId   INT;
DECLARE @CatId       INT;
DECLARE @ModelText   INT;
DECLARE @ModelImage  INT;
DECLARE @AppId       INT;
DECLARE @Now         DATETIME2 = GETUTCDATE();

-- CreatorProfile فرزاد
SELECT @CreatorId = Id FROM CreatorProfiles
WHERE UserId = 'fe5dd038-9621-4675-a8e2-bd8e44123886';

IF @CreatorId IS NULL
BEGIN
    INSERT INTO CreatorProfiles (UserId, Bio, IsVerified, IsFoundingCreator, CommissionPercent, JoinedAt)
    VALUES ('fe5dd038-9621-4675-a8e2-bd8e44123886', N'سازنده تست', 0, 0, 70.00, GETUTCDATE());
    SET @CreatorId = SCOPE_IDENTITY();
END

SELECT TOP 1 @CatId = Id FROM Categories ORDER BY Id;

SELECT TOP 1 @ModelText = Id FROM AiModels
WHERE IsActive = 1 AND Capabilities LIKE N'%TextGeneration%' ORDER BY Id;

SELECT TOP 1 @ModelImage = Id FROM AiModels
WHERE IsActive = 1 AND Capabilities LIKE N'%ImageGeneration%' ORDER BY Id;

IF @ModelImage IS NULL SET @ModelImage = @ModelText;

PRINT N'CreatorId=' + CAST(@CreatorId AS NVARCHAR) +
      N'  CatId='   + CAST(@CatId     AS NVARCHAR) +
      N'  ModelText='+ CAST(@ModelText  AS NVARCHAR) +
      N'  ModelImage='+ CAST(@ModelImage AS NVARCHAR);

-- ════════════════════════════════
-- 1. متن تبلیغاتی  OutputType=0 (Text)
-- ════════════════════════════════
INSERT INTO Apps (Slug,Title,ShortDescription,Description,ThumbnailUrl,
    Status,CreditCost,OutputType,AiModelId,EncryptedPrompt,SystemContext,
    CategoryId,CreatorProfileId,ExecutionCount,AverageRating,CreatedAt,UpdatedAt)
VALUES (
    'ad-text-generator',
    N'تولید متن تبلیغاتی حرفه‌ای',
    N'متن تبلیغاتی جذاب و متقاعدکننده برای محصول یا خدمت شما بسازید',
    N'با وارد کردن اطلاعات محصول، در چند ثانیه یک متن تبلیغاتی کامل شامل تیتر، متن اصلی، CTA و کپشن شبکه‌های اجتماعی دریافت کنید.',
    NULL, 2, 1, 0, @ModelText,
    'UC5E7ZrXEcIHA2GnzIgsd1ZSpGB1dc5ri+Eqkca94TX55BdA3HX+JlZ019SPqSLatzFlFWgI7TsaM/PG8/6HekHouevBOhGcfhAE84bDzbMg8iTME/AgqnLdmQAKPtpIuL0p2rn0E3zDGx8AiWwBNbg4Y2cxqaZENOPhZuV91x/mE9QDJlWrupwDD2hZHnUrDu9hVsf8ikkxbDSfb4ds5gi3e3lzh9IKy8zKfAWi8EqZWXU8lg0dZXb/SszqTbvbb50Y/LLZnzDMoz982vgS8Ktl/9cW3DkHyYKpG4carloBEZx32yJ/pMj0jBtTSrKSAI3IrQUBaOptWI4M43iU1oqQy5jyUxGVUs84Oapt1n62s/9y+C4BjWPhoG5gtwQrt0HMDYUouVbZ8f4xBUFcU5EFV/WxjYtIr+DT65W7GUmo760NI/z1k+sM8jmrs6IyOSs4Rn8II5GN7xCQd0njFjQ4t12fguEDJ1VJoj7yCRd4+0f8Yz3+U+hYM0nn74Wqn1hqCG92RTRMjTgz/LOBLq54hb0xt81dSddoP8nolvD7jWnVsqst9ndY9nebdIpqXCSpTtgrOSwzYlwvSK25OcH5fsxvf4galYSk++qrtVwZmWYuplPIoz5FgmtsYPM+KXqorlFhKjyfZFF4Oqza/vKU/Q+bFI0LQzHYVggfi1ZtM+1iSsZzdt02OprH8iMGMd+YLrdsWi+AjrFsVTQN3033jjziS0PCWq0DzbVOmK4oCn1SXVOOmGvdwixbn5+zPnmggDtlkVqVUQKQxjM7iD8JfkdDPdcdYNcqyBnWuquRYXf24+kU1eCKlLJs5s8nSW/UihgqsJE9NwpJkvZmREWw6rgoBF80PwNKzLul1SEY9mAwE/QMwXiyXUYjvEIB09I/WUwiVbKV8SLqnf22FewF7qIZqbRC/Mn5ehihDUIw2vahya/hD6O8+0EIxPAJ9ihquPZ7nsWDJspRv8UclVgvbU48vWRW4CI2cj/D4lHKXhVPjgf6TppMdQEZJJArwtdQGq+qm5GtzGjw0135KdadORCbE583vdpx1HP+JhaYCdmOqJpD+nextUlTb4hyi0wB3jyC1eHRwZGWvUAgjTl/Yhgbr7uSvDWszHp3nbo=',
    N'یک دستیار کپی‌رایتر حرفه‌ای هستی که متون تبلیغاتی جذاب به فارسی می‌نویسی.',
    @CatId, @CreatorId, 0, 0.00, @Now, @Now
);
SET @AppId = SCOPE_IDENTITY();

INSERT INTO AppInputFields (AppId,Name,Label,Placeholder,HelpText,Type,Options,IsRequired,MinLength,MaxLength,SortOrder) VALUES
(@AppId,'product_name',   N'نام محصول/خدمت',  N'مثال: اپلیکیشن تاکسی آنلاین', N'نام دقیق محصول یا خدمتی که می‌خواهید تبلیغ کنید', 0,NULL,1,2,100,1),
(@AppId,'features',       N'ویژگی‌های اصلی',  N'سریع، ارزان، امن...',           N'مهم‌ترین مزایا و ویژگی‌های محصول را بنویسید',        1,NULL,1,10,500,2),
(@AppId,'target_audience',N'مخاطب هدف',        N'جوانان ۱۸ تا ۳۵ سال',          N'چه کسانی مخاطب اصلی این محصول هستند؟',             0,NULL,1,3,200,3),
(@AppId,'tone',           N'تون برند',         NULL,                              N'لحن مناسب برند را انتخاب کنید',                     2,N'["رسمی","دوستانه","هیجان‌انگیز","الهام‌بخش","طنز"]',1,NULL,NULL,4);

INSERT INTO AppTags (AppId,TagName) VALUES
(@AppId,N'تبلیغات'),(@AppId,N'بازاریابی'),(@AppId,N'کپی‌رایتینگ'),(@AppId,N'محتوا');

-- ════════════════════════════════
-- 2. مولد پرامپت تصویر  OutputType=1 (Image)
-- ════════════════════════════════
INSERT INTO Apps (Slug,Title,ShortDescription,Description,ThumbnailUrl,
    Status,CreditCost,OutputType,AiModelId,EncryptedPrompt,SystemContext,
    CategoryId,CreatorProfileId,ExecutionCount,AverageRating,CreatedAt,UpdatedAt)
VALUES (
    'ai-image-prompt-generator',
    N'مولد پرامپت تصویر هوش مصنوعی',
    N'پرامپت حرفه‌ای برای Midjourney، DALL-E و Stable Diffusion بسازید',
    N'ایده تصویری خود را وارد کنید و یک پرامپت انگلیسی حرفه‌ای کامل با Negative Prompt و پارامترهای پیشنهادی دریافت کنید.',
    NULL, 2, 2, 1, @ModelImage,
    '50cvo9KsAQOhEs6Yh02Ev0LawNyq3ryCyWUu0Z+Gesn02ecF1n+NDsEq9G9BY0Q/OiVQLUpToo7ok6KGmVmh8/e1pEYpRR5MXlvA6bE7Wj7Be4W5pc3hRm/IWgmCEsJlJ4bfH5nELwBNwaK1aVdDKcP95z11M+iAAaQe6znv4eDt4iekZ/EpE5ZaMXnDo65MYYD44lHT4aL6RcSMZ+UBTiqW2NB4+mMZA54nRy7U39cl7jTSayAXVsvADUAaQbBGk1Ram1dDLvU0h8RU9JImp20zw7Ki5qOOYjcGU0RMVBEVZpo5VdlBDeHJO9H3yP4tvMOC5hYURsQLenCnmyLwPSUF/7qT5oKxOIAI2v+1C0cmCpujJswIqcyYXpoB7k+INnFukHe2c0gOQr1PRuJ8/OXJzmnpJWhIsdkmv0L43wxqp1oPVUiTGR+S5dg9x+BmOR60cmSy+F45+dU921ZeXFlxNJTi0PTIRWOcngoi/LG3r/yii9lEEkeBCkjL3qNLqGOp3L9Fwv8xE9Bd/iA2vabLyNpjdHk7r9EIz2YNgj56e4eizHxIL1zHVwhR61UCZ7gpg45R2Ac44wtxlCbes+FBsIpk5h0dgLTmRWcr4TxhLER9o9ycCOa+DDu7qgvIruTBUIOK5pK+3ArRhBin7adv4ndPODeScOZeybSdJ5QFrELuNZjEIEhufactLRdUaD5oSWaLmzBUhoxGMNYx6MzhQL8fYhZyXPqfny5zw98oY3QZPgsRh8Y7ix/qOamuW/nCG5cFaOBB6+TSNlEDGE2vqEKsb7Lq9YppMWHelwzkoLXZZwgbJLjOgHvwu/lSuk4cnsn/ZI8DYkFEL2InR5l5vkdUUDM1k5qmHQ4CmdLWss487vlSJKH9Pyd5l+z2CHJhrEOBOkVl3PZoW4QsNqUAP6KElIgJPE8xt1cR1sYvU4CRoY4FEeMOL6sI6te/Tq6Vs37ovZkrc0pJVEvMLCt4kNwWASy5hSZFbcc8Q3FWsPDMN8wwTUby1qOnVB3Nf+zHAOiV7ACsjf5+RJGeDi5wkdL1+fWlN6oTPfuEEqtqXXlPTmobYW6vPYPsi2SX3Btmytf6Po110/ilPrZtOO3UbQqNLaJ1B0lwoHgl/t/QFfkjUVjn+lEaKlQAN8i4',
    N'یک متخصص prompt engineering هستی که برای مدل‌های تولید تصویر پرامپت‌های حرفه‌ای می‌نویسی.',
    @CatId, @CreatorId, 0, 0.00, @Now, @Now
);
SET @AppId = SCOPE_IDENTITY();

INSERT INTO AppInputFields (AppId,Name,Label,Placeholder,HelpText,Type,Options,IsRequired,MinLength,MaxLength,SortOrder) VALUES
(@AppId,'subject',  N'موضوع اصلی',  N'مثال: یک پیرزن در باغ گل',           N'موضوع تصویری که می‌خواهید بسازید را توصیف کنید', 0,NULL,1,3,200,1),
(@AppId,'art_style',N'سبک هنری',    NULL,                                    N'سبک هنری مورد نظر را انتخاب کنید',               2,N'["Photorealistic","Digital Art","Oil Painting","Watercolor","Anime","Cinematic","Minimalist","Surrealism"]',1,NULL,NULL,2),
(@AppId,'colors',   N'رنگ‌بندی',    N'مثال: گرم، طلایی، کنتراست بالا',     N'پالت رنگی یا ترکیب رنگ مورد نظر',               0,NULL,0,0,100,3),
(@AppId,'mood',     N'حالت/احساس',  NULL,                                    N'احساسی که می‌خواهید تصویر منتقل کند',             2,N'["Epic","Peaceful","Dark","Joyful","Mysterious","Romantic","Melancholic"]',1,NULL,NULL,4);

INSERT INTO AppTags (AppId,TagName) VALUES
(@AppId,N'تصویر'),(@AppId,N'Midjourney'),(@AppId,N'هوش مصنوعی'),(@AppId,N'DALL-E');

-- ════════════════════════════════
-- 3. مولد کد  OutputType=4 (Code)
-- ════════════════════════════════
INSERT INTO Apps (Slug,Title,ShortDescription,Description,ThumbnailUrl,
    Status,CreditCost,OutputType,AiModelId,EncryptedPrompt,SystemContext,
    CategoryId,CreatorProfileId,ExecutionCount,AverageRating,CreatedAt,UpdatedAt)
VALUES (
    'code-generator-pro',
    N'مولد کد برنامه‌نویسی حرفه‌ای',
    N'کد تمیز، مستند و بهینه در هر زبان برنامه‌نویسی دریافت کنید',
    N'توضیح دهید چه کدی نیاز دارید و در چند ثانیه یک کد کامل با کامنت، مثال استفاده و تحلیل پیچیدگی دریافت کنید.',
    NULL, 2, 3, 4, @ModelText,
    'MZJxfC2jYmUVQAQ7Ra7+GImq/GLN9JV3if8C44R4aRWlxjoUul2GN3FyXh9dz16b9gBcf4nmT9kn/66JWC39HKIIPwhG3k/urrUfs3eDj9vqVl+FrPM0TUltFEXuQD58zFIu2QICzZ5NhFPYW3p+bPnAu2NFRotgbqw7t6sgYTDv73jfnOCFxLm4l+gDemxT0LKjqMC4lhqvW5oQdT3q9Fu1VrXFVNaSxKRQVrTsnw/vLKIs09sDhB50bp4WA3pE0wnHCYFSgbo87P2QlC5m6t5zAmY5cTVGHyy+8OAaqSPH7Qkm6UpUuK6zGDvs1tZ0gzIVef1RR3xN1rHp8EjLfU7I/Ld/I5FIVz9mkmnW0RMp7kZK5BGR9Uv06+Vk6s0SO7whOOc9WI38NeR8XlE58y7Y/do35YLW9X++Thtsjm4M0THAfzdSe0UZeb58KFUd9wq9dYt0hoqcN74sy/bnG783b3LvaYiUrXK6S+gLoJrr6wapoVgyYMuDVMj+bE7cFc6MYjVCCUS4zAQPFXLHe+hyKV2Y3LMgtbdDjIzUaY8JQraQJeQLgXE2oGDS2aZKBM3AC7AihJWmOYWl+43RwKPxt9ncvG4aAkB3lSZIHOGPn5STXtFjci8wlD/J5WgNRDJ6iki21byLS063DGHxoLy2sLOWr5JI+N5I5TvfmcgWInera1Kkqu6MS6BTW92A6LGedLTfTSc1vyM+purwrxa3Gigs2IZ5xi0GG5J5pHw9/Xwzf+wisTn/fhN1zQMErG4XumiNxh3hyDegomuiE+UPNZFjfR2JAsf1REnbnrIqiRPVlEo9HiqtXJr/yYdFiHoFCmTtgaGqJNyxSpFX0/O/CPm/nvIpn99QBtwn+UtJD+rwmX39V2o63hzvBmVkWgUf7vSWLbrL16Wo8p/P6HHR5y7LftHSyUzQXiJ/snyeH1g8BluwHBtSgRmqOms5bftesKoMxq/9svbSWLy02Qrjyob+SdAsUHyidnDLT6wOoKLJpqC8BNBRQYMxJjxBC6dCh1ldtVIZTNMLt1yEPz3hh3r8CjF4iSoFHT83M/WvaxR5B3/xg5GNtsYsCa2lT5Yp/gsMGpX/rw0YL4nnxQ==',
    N'یک مهندس نرم‌افزار ارشد هستی که کدهای تمیز، بهینه و مستند می‌نویسی.',
    @CatId, @CreatorId, 0, 0.00, @Now, @Now
);
SET @AppId = SCOPE_IDENTITY();

INSERT INTO AppInputFields (AppId,Name,Label,Placeholder,HelpText,Type,Options,IsRequired,MinLength,MaxLength,SortOrder) VALUES
(@AppId,'language',       N'زبان برنامه‌نویسی', NULL,                                        N'زبانی که کد باید در آن نوشته شود',                 2,N'["Python","JavaScript","TypeScript","C#","Java","Go","Rust","PHP","Swift","Kotlin"]',1,NULL,NULL,1),
(@AppId,'description',    N'توضیح عملکرد',      N'مثال: تابعی که لیست اعداد را مرتب می‌کند', N'دقیق توضیح دهید کد چه کاری باید انجام دهد',         1,NULL,1,10,1000,2),
(@AppId,'inputs',         N'ورودی‌ها',           N'مثال: list of integers',                    N'ورودی‌های تابع یا برنامه',                           0,NULL,0,0,300,3),
(@AppId,'expected_output',N'خروجی مورد انتظار', N'مثال: sorted list, boolean',                N'چه خروجی‌ای انتظار دارید؟',                         0,NULL,1,2,200,4);

INSERT INTO AppTags (AppId,TagName) VALUES
(@AppId,N'برنامه‌نویسی'),(@AppId,N'کد'),(@AppId,N'توسعه'),(@AppId,N'Python');

-- ════════════════════════════════
-- 4. تحلیل رقبا  OutputType=3 (Form)
-- ════════════════════════════════
INSERT INTO Apps (Slug,Title,ShortDescription,Description,ThumbnailUrl,
    Status,CreditCost,OutputType,AiModelId,EncryptedPrompt,SystemContext,
    CategoryId,CreatorProfileId,ExecutionCount,AverageRating,CreatedAt,UpdatedAt)
VALUES (
    'competitor-analysis-tool',
    N'ابزار تحلیل رقبا (SWOT)',
    N'تحلیل رقابتی جامع و ساختاریافته برای هر کسب‌وکار',
    N'اطلاعات شرکت رقیب را وارد کنید و یک تحلیل SWOT کامل با استراتژی پیشنهادی و امتیاز رقابتی دریافت کنید.',
    NULL, 2, 3, 3, @ModelText,
    'ZQmMoUvI2NeJpweCt3Gj8HfeaMmXn9jOa5nS1bXF5ORuNcOyevtyznVSQzjsb0YVYPMju59NikStBIT1NfjCjJA0+mZxbqlXzmQPn/zyo0fDIkoyO/p4W+Femkgkm/1YilY2xheEBkVdxJCSpHsj0n0nZPKRjRALXT5UR39QQJhMmFUjNKqkmxtyb3mDPlfGQiyh3V4fq4VSnctf3lgJRa2iCScs/4ZWbZd8al8StMBX4i7XNZy7YMg/GQLX48MXep5/JBvJiwBTcWpE5IhZUtk+YOFXUgwjrLtS8Cz6TTM/gltKDsbYdt1KF3qR/tvkE3Y+GKlLoDOoC5DG+KIjBLNUThAK3Q6rb/bFMemwJH1HovkqTf7DwKwd9qGAx4NwvnaWoG+YmBGfKt41j0mfs+rtW1y/5CmjKFFmoGjynKoVfyhKA2oCAk0+NJWCV6SXb4TMT1trFcyOnnxZsDPGrdsF8YsJCnxHsYq+gdZYjJVjHp588tYnHxmsTjMuuORTH5sUgCWZfEE8iGfbn+UKpJjFr7tClcHH4OzLfYfJLF1w67pHPBFuPpRGfpWX1dDfsbArrFzJ16tB07/1jak5cuqIJ7XbJfvNAWgu2uAOKz6RelwzSevOLgFtHHLdOY78K1nQn6Z7puphhe07GMW69Oi4ql2x6F90MTwSAKtwstSq6xB/9vEMZeomolkY6kKPBgxSwWxTI14bT+gCpZzsKhdULAY3qZ7RgrZWwASBT4pRsi5JHO3srlKxX7UWKSEhD2/AhjoFjXxV1JGqx/0fJckFo7xrve6LFnp6Fudr14c3HIkoevDiRmrPfxADtiycAc2NjA/WFIeq2f7H8zxXOBTvVBpLGHxo2F5je/JvAZ+uR68hHmaoFjvm6PKJ1U6T4mGa5BlToSY6aTc9RmyExUTX8kdyP43ivpHEWDdgfbkiUSDONO9IZrodwHRR2YxJe/4aoc9BofwPz+ZKEefhniybXjGxVWSLYb0oi9ng+fFp420dQmNuzI00/gE00uAiryOqJOWdes3uWNxgvHHfCzpFm87P8KB6OvlBYJxDP4nF7n05WQ2xfbgdtH8vkCUVfVNV1sJ8A+F7eseH5fhK4lJzhqT7apG6xcGECdmmOx4nBU13v2HXanP+4RjjQRBiZG01AXGqVuExOwGE1BpM0QFXscb96/HoQVZXSHtZ8ZCQYosz3uxGTByogbOCm8r+gondf1oqZ6NkIcn6lQHBv6o8shwBV4aRHrRVIbufHXQ=',
    N'یک مشاور استراتژی کسب‌وکار هستی که در تحلیل رقابتی و SWOT تخصص داری.',
    @CatId, @CreatorId, 0, 0.00, @Now, @Now
);
SET @AppId = SCOPE_IDENTITY();

INSERT INTO AppInputFields (AppId,Name,Label,Placeholder,HelpText,Type,Options,IsRequired,MinLength,MaxLength,SortOrder) VALUES
(@AppId,'company_name', N'نام شرکت رقیب',      N'مثال: دیجی‌کالا',           N'نام دقیق شرکتی که می‌خواهید تحلیل کنید',        0,NULL,1,2,100,1),
(@AppId,'industry',     N'صنعت/حوزه فعالیت',  N'مثال: فروشگاه اینترنتی',   N'صنعتی که شرکت در آن فعالیت می‌کند',              0,NULL,1,2,100,2),
(@AppId,'main_product', N'محصول/خدمت اصلی',   N'مثال: خرید آنلاین کالا',   N'مهم‌ترین محصول یا خدمت این شرکت',                 0,NULL,1,3,200,3),
(@AppId,'target_market',N'بازار هدف',           N'مثال: مصرف‌کنندگان ایرانی',N'مشتریان اصلی این شرکت چه کسانی هستند؟',          0,NULL,1,3,200,4);

INSERT INTO AppTags (AppId,TagName) VALUES
(@AppId,N'تحلیل'),(@AppId,N'SWOT'),(@AppId,N'کسب‌وکار'),(@AppId,N'استراتژی');

-- ════════════════════════════════
-- 5. اسکریپت پادکست  OutputType=5 (Audio)
-- ════════════════════════════════
INSERT INTO Apps (Slug,Title,ShortDescription,Description,ThumbnailUrl,
    Status,CreditCost,OutputType,AiModelId,EncryptedPrompt,SystemContext,
    CategoryId,CreatorProfileId,ExecutionCount,AverageRating,CreatedAt,UpdatedAt)
VALUES (
    'podcast-script-writer',
    N'نویسنده اسکریپت پادکست',
    N'اسکریپت کامل و حرفه‌ای برای اپیزود پادکست شما بسازید',
    N'موضوع و مشخصات پادکست خود را وارد کنید و یک اسکریپت کامل شامل Intro، محتوای اصلی، سوالات تعاملی و Outro دریافت کنید.',
    NULL, 2, 2, 5, @ModelText,
    'qOcgAU0ARaUEa85h3rzYevZrMUwOgb6EU6iPVRTCzKcpnFjTsi5bZw/lALdoApw2cM4tU3puD50ZUTtQvqPZdx6LqG7xTR6zk/cV2+iiqCQnMvwnN+If0Wxuhr412nM9IFOWI+E5trzhlQNNnXEsOLXYl3/tMFW9csJBqV49mw3RNkrQt7Xqta9azaZKoiF/0iznH5Kh0IlppAaeZSFvttko3FMCGmS8R7iYTi5R7ruyVUtGYqLaSEIbu9sYQpyA9YOZ5V+OGLjyU486e49JBE+rzFO+ctuGgTOiGu3r1aL3L9wJJn7ah7w5zfO+BqxCXn/OK6jqW2GK8oram1qNlAL8mgrm0xmSJ/O66/Q52JAYD9LoXob7dddkg2WGY4ysLK5D6rj01W56vy+eiejrEsNFezKyCUjwA+Nw6abXZ8ffyN1WNre+juRyYiFpOJayO/FBeACiiKEEgymGmS4tmg5unAu7X2smh/he9mN4tPher7f/UI3G0YL6gS8nDVQ9PDpfDGELk28e196IfwJTtTnGz1bENMhgqffuUXM4QAg1sOfi+GGItcvKubuA+j8Xnqg95G7C62o2nFS3Si/UYD7PF7NVZ2k+cx2Wc6E4XbR0q/6tqH5wGrUE+ryUPkV7ShQ2IhQgWK+H9fmOjIS6LxAZTou1ta3NGc9UQ1QOrg68vG6YVFON9OLcv8ZFC5uj9aqBvrIzs1RM2zsQRw7mqPJ1YBnrFUMT8lGuzzh11S/sjb+hD6aQ6FwSNWNSaz0scsh2dqv/SX9GQCTLpXXMxScdzRvRVAKWB+3JCf/HBepCa3o5tSwsIr/vm1pc/KPJOng4QiV3QR0ndwj/iEmsVEavNxEoJzvpmMJ3mtLRT2Pxi+557XX10uclR0Tgj65C7SIETTKIKT13DGpJlBfo/YkkEv21K0vnE3e3qeakizKp9OFr3qvRrfxKPS1b0pj8V5u6+k4DSRdSbOoljr35ZFcHiVWDvSCXaJrO28qKjUd9LgOT8T2JW5wxyjbgeIlBpMZ8BBQUvo6NFvih/EhcWRpUg3OBcc7u1oTzbINzgZ8/jwqvx9TvY8FdYk/9sEOw9eGWiPAstEjrhMB02FhOYfD2Lgnldq/PqFdnr+pa7jKqQnkM7aQSDpL68xSD/S52SzlFJ/5Nc6ML1V1cEfEIS6pM60YU5+jQNzDxXgem5RV+JOkXwPlxuM0u9/NiaXQU',
    N'یک تهیه‌کننده محتوای پادکست حرفه‌ای هستی که اسکریپت‌های جذاب و شنیدنی می‌نویسی.',
    @CatId, @CreatorId, 0, 0.00, @Now, @Now
);
SET @AppId = SCOPE_IDENTITY();

INSERT INTO AppInputFields (AppId,Name,Label,Placeholder,HelpText,Type,Options,IsRequired,MinLength,MaxLength,SortOrder) VALUES
(@AppId,'topic',   N'موضوع پادکست',     N'مثال: آینده هوش مصنوعی در ایران', N'موضوع اصلی این اپیزود پادکست',                   0,NULL,1,5,200,1),
(@AppId,'duration',N'مدت زمان (دقیقه)', NULL,                                 N'چند دقیقه می‌خواهید اسکریپت داشته باشید؟',       2,N'["۵","۱۰","۱۵","۲۰","۳۰","۴۵","۶۰"]',1,NULL,NULL,2),
(@AppId,'style',   N'سبک پادکست',       NULL,                                 N'لحن و سبک ارائه محتوا',                          2,N'["آموزشی","گفتگومحور","داستانی","تحلیلی","انگیزشی"]',1,NULL,NULL,3),
(@AppId,'audience',N'مخاطب هدف',        N'مثال: کارآفرینان جوان',            N'چه کسانی به این پادکست گوش می‌دهند؟',             0,NULL,1,3,150,4);

INSERT INTO AppTags (AppId,TagName) VALUES
(@AppId,N'پادکست'),(@AppId,N'محتوا'),(@AppId,N'صوتی'),(@AppId,N'اسکریپت');

-- ════════════════════════════════
-- 6. سناریوی ویدیو  OutputType=2 (Video)
-- ════════════════════════════════
INSERT INTO Apps (Slug,Title,ShortDescription,Description,ThumbnailUrl,
    Status,CreditCost,OutputType,AiModelId,EncryptedPrompt,SystemContext,
    CategoryId,CreatorProfileId,ExecutionCount,AverageRating,CreatedAt,UpdatedAt)
VALUES (
    'video-script-creator',
    N'سازنده سناریوی ویدیو',
    N'سناریوی ویدیو آماده فیلمبرداری برای هر پلتفرم بسازید',
    N'مشخصات ویدیوی خود را وارد کنید و یک سناریوی کامل صحنه به صحنه با Hook، CTA، ایده موسیقی و هشتگ دریافت کنید.',
    NULL, 2, 2, 2, @ModelText,
    '0ChEhlcCJK1fUf/EJkbusBBvLxUjK/vRwZ3/RJfoKRli5LIFx52qahNpu5rkHgqEp0wjPzNQcCpgjMwij10bG0eL4v9wtvmPywiwCFM+PTNGshjdOQ6Q1cTu/Y9mDkGxI/5NhP+XjyTjLW7X3kyNCXTj0vpRvWh4wGgyXRbDfLGW9qFDl8ViSLWTj0+z2thwi+NoBZkb+duq7CQOLST8uJ5JiVMi7vm8avuB+D3C2PrptG39RyntYOOA4MsjDvcfLCBfon/dotpvkpiW+tLx521ZA2O83vcodIwhlW4xKazSN05nH/GEMyAankKeh0w/OOcqKSRyafsTYyAmI8AKccarSXXFVzib95U4/g/xvDCo6RcvcJdJuIAW3hFdgf32ZvOZoX173QEKnv+sBJoFxkecP35fO5g350eJwFLUed/+4YaPjGh8dQxY+Y/pQT22OybL+Hc/ufTZ+znYv9c//nolYPdX5Tt45fbkO9caONquKvpYDzybeuSDASVMm0sv4lMkfQYMqzNoSk8jAjclHZRfkzvoKx8tVikOkUKSoBpBjAY9ZnoIjI5NguUOzLTKDsjWoH9fIz19OWFPobsdvxDI0Eqeaz8JShRyZWiHV4ppIZPCjvic+3hXBjjPu2Aj+tGORTRei7l19RxKEGP1xgLn863iGU8pwY8yNLUrB8XEfIN9uAzRIsZ3NPMjHet+dFf/toLNDshF1yIVMl8bM34Oyn4tZKxWcFwKO2aFQ9sYVmX9hIEhyQmpK81XyQuETHzncjdK58fBTAMVz8cMeuCGK1IrSq3XusxeXHcq3NfPwFJvCiYo84ue2gdJrgrund9AxWqoH6VGxXTsJcudZ1OfbmG1xDKGzeKsnh9Xc7KdumvwAjBcE5vmOiGw6a0Qy1cteP71a2fxycoxaVQ8FhWJGuKcBHre3zvVS9bm52hAmrdEHnV3E/TO8eoHVPREmQsqENShQH3Y4J8LczQ/ZHlvELgIQhoGnaehgVTfrCjXD0nUOA0TuEFXr48Gqf1qY6Nw5MPy9664gBEvpOHR71kyVsqy2DkL+r6eMeTKLE4ShMSPbqvF5aP15hVUbC4HrCKK1nIjLB0M4LCW2nDq06LA6iNOd5yp8G+YOb0PMLx9V3Ot9KDeaJgLTfk+eXj4',
    N'یک کارگردان و نویسنده سناریوی ویدیو حرفه‌ای هستی که برای پلتفرم‌های مختلف محتوا می‌سازی.',
    @CatId, @CreatorId, 0, 0.00, @Now, @Now
);
SET @AppId = SCOPE_IDENTITY();

INSERT INTO AppInputFields (AppId,Name,Label,Placeholder,HelpText,Type,Options,IsRequired,MinLength,MaxLength,SortOrder) VALUES
(@AppId,'topic',   N'موضوع ویدیو', N'مثال: معرفی محصول جدید',             N'موضوع اصلی ویدیو را بنویسید',                    0,NULL,1,3,200,1),
(@AppId,'platform',N'پلتفرم',       NULL,                                   N'ویدیو برای کدام پلتفرم ساخته می‌شود؟',            2,N'["اینستاگرام ریلز","یوتیوب","آپارات","تیک‌تاک","لینکدین","تلگرام"]',1,NULL,NULL,2),
(@AppId,'duration',N'مدت زمان',     NULL,                                   N'مدت زمان ویدیو',                                  2,N'["۳۰ ثانیه","۶۰ ثانیه","۳ دقیقه","۵ دقیقه","۱۰ دقیقه","۱۵ دقیقه"]',1,NULL,NULL,3),
(@AppId,'goal',    N'هدف ویدیو',    NULL,                                   N'چه هدفی از این ویدیو دنبال می‌کنید؟',              2,N'["افزایش فروش","آگاهی از برند","آموزش","سرگرمی","ویروسی شدن"]',1,NULL,NULL,4);

INSERT INTO AppTags (AppId,TagName) VALUES
(@AppId,N'ویدیو'),(@AppId,N'سناریو'),(@AppId,N'محتوا'),(@AppId,N'یوتیوب');

-- ════════════════════════════════
COMMIT TRANSACTION;
PRINT N'6 ابزار تست با موفقیت ایجاد شدند.';
