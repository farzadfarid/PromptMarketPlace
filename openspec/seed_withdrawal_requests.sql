-- ============================================================
-- داده فیک برای تست صفحه درخواست‌های برداشت ادمین
-- پیش‌نیاز: حداقل یک CreatorProfile در جدول CreatorProfiles موجود باشد
-- اجرا: SQL Server Management Studio یا هر کلاینت SQL
-- ============================================================

-- ابتدا ID سازنده‌های موجود رو می‌بینیم
-- SELECT Id, UserId FROM CreatorProfiles;

-- اگر می‌خوای با ID خاص کار کنی، عدد رو جایگزین کن
-- در مثال زیر از CreatorProfileId = 1 استفاده شده

DECLARE @CreatorId INT = (SELECT TOP 1 Id FROM CreatorProfiles ORDER BY Id);

-- اگر هیچ سازنده‌ای نداری، این بلاک رو uncomment کن و ID کاربر خودت رو بزار
-- INSERT INTO CreatorProfiles (UserId, Bio, IsVerified, CommissionPercent, JoinedAt)
-- VALUES ('ID-کاربر-سازنده', N'سازنده تست', 1, 70, GETUTCDATE());
-- SET @CreatorId = SCOPE_IDENTITY();

-- ============================================================
-- 1. در انتظار بررسی (Pending = 0)
-- ============================================================

INSERT INTO WithdrawalRequests (CreatorProfileId, Amount, Status, BankAccountInfo, AdminNote, CreatedAt, ProcessedAt)
VALUES
(@CreatorId, 2500000, 0,
 N'{"sheba":"IR120570028080010000000000","accountOwner":"علی محمدی"}',
 NULL,
 DATEADD(DAY, -2, GETUTCDATE()), NULL),

(@CreatorId, 1800000, 0,
 N'{"sheba":"IR550570028080010000000001","accountOwner":"علی محمدی"}',
 NULL,
 DATEADD(DAY, -1, GETUTCDATE()), NULL),

(@CreatorId, 4200000, 0,
 N'{"sheba":"IR340170000000105269000000","accountOwner":"علی محمدی"}',
 NULL,
 DATEADD(HOUR, -5, GETUTCDATE()), NULL);

-- ============================================================
-- 2. پرداخت شده (Paid = 3)
-- ============================================================

INSERT INTO WithdrawalRequests (CreatorProfileId, Amount, Status, BankAccountInfo, AdminNote, CreatedAt, ProcessedAt)
VALUES
(@CreatorId, 3000000, 3,
 N'{"sheba":"IR120570028080010000000000","accountOwner":"علی محمدی"}',
 N'شناسه پرداخت: 1402031500012345',
 DATEADD(DAY, -15, GETUTCDATE()),
 DATEADD(DAY, -14, GETUTCDATE())),

(@CreatorId, 5500000, 3,
 N'{"sheba":"IR120570028080010000000000","accountOwner":"علی محمدی"}',
 N'شناسه پرداخت: 1402021800087654',
 DATEADD(DAY, -30, GETUTCDATE()),
 DATEADD(DAY, -29, GETUTCDATE())),

(@CreatorId, 1200000, 3,
 N'{"sheba":"IR120570028080010000000000","accountOwner":"علی محمدی"}',
 N'واریز شد — شناسه: 1402011200054321',
 DATEADD(DAY, -45, GETUTCDATE()),
 DATEADD(DAY, -44, GETUTCDATE()));

-- ============================================================
-- 3. رد شده (Rejected = 2)
-- ============================================================

INSERT INTO WithdrawalRequests (CreatorProfileId, Amount, Status, BankAccountInfo, AdminNote, CreatedAt, ProcessedAt)
VALUES
(@CreatorId, 900000, 2,
 N'{"sheba":"IR000000000000000000000000","accountOwner":"علی محمدی"}',
 N'شماره شبا اشتباه است. لطفاً مجدداً درخواست دهید.',
 DATEADD(DAY, -10, GETUTCDATE()),
 DATEADD(DAY, -9, GETUTCDATE())),

(@CreatorId, 7000000, 2,
 N'{"sheba":"IR120570028080010000000000","accountOwner":"علی محمدی"}',
 N'مغایرت با موجودی کیف پول. درخواست معتبر نیست.',
 DATEADD(DAY, -20, GETUTCDATE()),
 DATEADD(DAY, -19, GETUTCDATE()));

-- ============================================================
-- 4. تایید شده ولی هنوز واریز نشده (Approved = 1)
-- ============================================================

INSERT INTO WithdrawalRequests (CreatorProfileId, Amount, Status, BankAccountInfo, AdminNote, CreatedAt, ProcessedAt)
VALUES
(@CreatorId, 3800000, 1,
 N'{"sheba":"IR120570028080010000000000","accountOwner":"علی محمدی"}',
 N'تایید شد — در صف واریز',
 DATEADD(DAY, -3, GETUTCDATE()),
 DATEADD(DAY, -2, GETUTCDATE()));

-- ============================================================
-- نمایش نتیجه
-- ============================================================
SELECT
    wr.Id,
    wr.Amount,
    CASE wr.Status
        WHEN 0 THEN N'در انتظار'
        WHEN 1 THEN N'تایید شده'
        WHEN 2 THEN N'رد شده'
        WHEN 3 THEN N'پرداخت شده'
    END AS [وضعیت],
    wr.CreatedAt,
    wr.ProcessedAt,
    wr.AdminNote
FROM WithdrawalRequests wr
WHERE wr.CreatorProfileId = @CreatorId
ORDER BY wr.CreatedAt DESC;
