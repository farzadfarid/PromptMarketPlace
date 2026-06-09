# ChatQT API Reference

**Base URL:** `https://api.chatqt.com/api/v1`  
**Auth:** `Authorization: Bearer $CHATQT_API_KEY`  
**قیمت‌گذاری:** دلاری  
**کنسول:** console.chatqt.com

---

## مشخصات کلی

ChatQT یک gateway یکپارچه است که همه درخواست‌ها — از جمله تولید تصویر — را از طریق **`chat/completions`** مدیریت می‌کند. endpoint های جداگانه مثل `/images/generations` یا `/video/generations` **وجود ندارند**.

---

## Text Generation

```
POST https://api.chatqt.com/api/v1/chat/completions
Content-Type: application/json
Authorization: Bearer $KEY
```

**Request:**
```json
{
  "model": "anthropic/claude-sonnet-4",
  "messages": [
    { "role": "system", "content": "You are a helpful assistant." },
    { "role": "user", "content": "Hello!" }
  ],
  "max_tokens": 2048
}
```

**Response:** استاندارد OpenAI
```json
{
  "choices": [{ "message": { "content": "Hi there!" } }],
  "usage": { "total_tokens": 42 }
}
```

---

## Image Generation (از طریق chat/completions)

```
POST https://api.chatqt.com/api/v1/chat/completions
```

**Request:**
```json
{
  "model": "openai/gpt-5-codex",
  "messages": [{ "role": "user", "content": "Generate an image of..." }],
  "max_tokens": 4096
}
```

**Response — فرمت اختصاصی ChatQT:**
```json
{
  "choices": [{
    "message": {
      "images": [{
        "image_url": {
          "url": "https://cdn.chatqt.com/..."
        }
      }]
    }
  }]
}
```

> ⚠️ URL تصویر ممکن است نیاز به Bearer auth برای دانلود داشته باشد

**مسیر استخراج URL:** `choices[0].message.images[0].image_url.url`

---

## مدل‌های موجود (48 مدل — 2026-06-09)

### Text / Code
| Model ID | نام |
|---|---|
| `anthropic/claude-opus-4.8` | Claude Opus 4.8 |
| `anthropic/claude-opus-4.7` | Claude Opus 4.7 |
| `anthropic/claude-opus-4` | Claude Opus 4 |
| `anthropic/claude-sonnet-4.5` | Claude Sonnet 4.5 |
| `anthropic/claude-sonnet-4` | Claude Sonnet 4 |
| `anthropic/claude-haiku-4.5` | Claude Haiku 4.5 |
| `openai/gpt-5.5` | GPT-5.5 |
| `openai/gpt-5.2-pro` | GPT-5.2 Pro |
| `openai/gpt-5.2` | GPT-5.2 |
| `openai/gpt-5.1` | GPT-5.1 |
| `openai/gpt-5` | GPT-5 |
| `openai/gpt-5-mini` | GPT-5 Mini |
| `openai/gpt-5-nano` | GPT-5 Nano |
| `openai/gpt-5-chat` | GPT-5 Chat |
| `openai/gpt-4.1` | GPT-4.1 |
| `openai/gpt-4.1-mini` | GPT-4.1 Mini |
| `openai/gpt-4.1-nano` | GPT-4.1 Nano |
| `openai/gpt-4o-mini` | GPT-4o Mini |
| `openai/o3` | o3 |
| `openai/o4-mini-high` | o4 Mini High |
| `openai/o3-deep-research` | o3 Deep Research |
| `openai/o4-mini-deep-research` | o4 Mini Deep Research |
| `openai/gpt-4o-search-preview` | GPT-4o Search Preview |
| `openai/gpt-4o-mini-search-preview` | GPT-4o Mini Search |
| `google/gemini-3.1-pro-preview` | Gemini 3.1 Pro Preview |
| `google/gemini-3.1-flash-lite` | Gemini 3.1 Flash Lite |
| `google/gemini-3-flash-preview` | Gemini 3 Flash Preview |
| `google/gemini-2.5-pro` | Gemini 2.5 Pro |
| `google/gemini-2.5-flash` | Gemini 2.5 Flash |
| `x-ai/grok-4.3` | Grok 4.3 |
| `deepseek/deepseek-v4-pro` | DeepSeek V4 Pro |
| `deepseek/deepseek-v4-flash` | DeepSeek V4 Flash |
| `deepseek/deepseek-v3.2-exp` | DeepSeek V3.2 Exp |
| `deepseek/deepseek-v3.1-terminus` | DeepSeek V3.1 |
| `deepseek/deepseek-chat` | DeepSeek V3 |
| `deepseek/deepseek-r1` | DeepSeek R1 |
| `moonshotai/kimi-k2.6` | Kimi K2.6 |
| `minimax/minimax-m2.7` | MiniMax M2.7 |
| `z-ai/glm-5-turbo` | GLM 5 Turbo |
| `z-ai/glm-5` | GLM 5 |
| `z-ai/glm-4.6` | GLM 4.6 |
| `qwen/qwen3.7-max` | Qwen 3.7 Max |
| `meta-llama/llama-4-maverick` | Llama 4 Maverick |
| `perplexity/sonar-pro` | Sonar Pro |
| `openai/gpt-5.1-codex` | GPT-5.1 Codex |
| `openai/gpt-5-codex` | GPT-5 Codex |

### Image Generation (با پشتیبانی تصویر در chat)
مدل‌هایی که badge **تصویر** دارند:
- `google/gemini-3-pro-image-preview` (Nano Banana Pro)
- `google/gemini-2.5-flash-image`
- `openai/gpt-5-codex`
- `openai/gpt-5.1-codex`
- و تعدادی از مدل‌های GPT-5 با badge تصویر

### ویدیو
❌ **پشتیبانی ندارد**

### صدا (TTS/STT)
❌ **پشتیبانی ندارد**

---

## Capabilities در PromptMarketPlace

| Capability | وضعیت |
|---|---|
| IsActiveForText | ✅ پشتیبانی می‌کند |
| IsActiveForImage | ✅ پشتیبانی می‌کند (via chat/completions) |
| IsActiveForVideo | ❌ پشتیبانی ندارد |
| IsActiveForAudio | ❌ پشتیبانی ندارد |

---

## نکات پیاده‌سازی

### Fallback Pattern برای تصویر
چون ChatQT از `/images/generations` پشتیبانی نمی‌کند:
1. ابتدا `POST images/generations` امتحان می‌شود
2. در صورت 4xx → `POST chat/completions` (fallback)
3. پارس response با مسیر `choices[0].message.images[0].image_url.url`

### دانلود تصویر
URL های CDN ChatQT ممکن است نیاز به Bearer auth داشته باشند:
```csharp
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
```

### Balance Check
```
GET https://api.chatqt.com/api/v1/token/usage?token={key}
Authorization: Bearer {key}
```
Response: `{ "status": "OK", "usage": 0.0001 }`  
واحد: USD

---

## تفاوت با AvalAI

| ویژگی | ChatQT | AvalAI |
|---|---|---|
| ویدیو | ❌ | ✅ (Sora, Veo) |
| صدا | ❌ | ✅ (TTS, STT) |
| قیمت | دلار | تومان |
| image endpoint | chat/completions | images/generations |
| تصویر response | `message.images[0]` | `data[0].url` |
