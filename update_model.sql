-- آپدیت Model ID به مدل رایگان فعال
UPDATE AiModels 
SET ModelId = 'google/gemma-4-31b-it:free',
    Name = 'Gemma 4 31B (رایگان)',
    MaxTokens = 2048
WHERE ModelId LIKE '%free%' OR ModelId LIKE '%llama%';

-- اگه مدل رایگان وجود نداشت، اولین مدل رو آپدیت کن
UPDATE AiModels 
SET ModelId = 'google/gemma-4-31b-it:free',
    Name = 'Gemma 4 31B (رایگان)',
    MaxTokens = 2048
WHERE Id = (SELECT TOP 1 Id FROM AiModels ORDER BY Id);

SELECT Id, Name, ModelId, MaxTokens FROM AiModels;
