# AvalAI API Reference

**Base URL:** `https://api.avalai.ir/v1`  
**Auth:** `Authorization: Bearer $AVALAI_API_KEY`  
**قیمت‌گذاری:** به تومان — گزینه ایرانی برای ویدیو، صدا و text

---

## Video Generation

### مشخصات کلی
- **async است** — ابتدا job ثبت می‌شود، سپس polling برای نتیجه
- endpoint استاندارد OpenAI (`/video/generations`) **وجود ندارد**
- endpoint اختصاصی: `/videos`

### مدل‌های موجود

| Model ID | توضیح | حداکثر مدت | قیمت/ثانیه |
|---|---|---|---|
| `sora-2` | کیفیت استاندارد | 20 ثانیه | $0.10 |
| `sora-2-pro` | کیفیت بالا | 20 ثانیه | $0.30 (عادی) / $0.50 (high-res) |
| `veo-3.1-generate-001` | Google Veo | - | - |
| `veo-3.1-fast-generate-001` | Google Veo Fast | - | - |
| `veo-3.1-generate-preview` | Google Veo Preview | - | - |
| `veo-3.1-fast-generate-preview` | Google Veo Fast Preview | - | - |
| `gen4.5` | - | - | - |
| `gen4_turbo` | - | - | - |

### ایجاد ویدیو

```
POST https://api.avalai.ir/v1/videos
Content-Type: application/json
Authorization: Bearer $KEY
```

**Request Body:**
```json
{
  "model": "sora-2",
  "prompt": "A calico cat playing a piano on stage under dramatic spotlights",
  "size": "1280x720",
  "seconds": "4"
}
```

| پارامتر | نوع | اجباری | توضیح |
|---|---|---|---|
| `model` | string | بله | ID مدل |
| `prompt` | string | بله | توضیح ویدیو — حداکثر 1000 کاراکتر |
| `seconds` | string | خیر | مدت (1-20) — پیش‌فرض "4" — باید string باشد |
| `size` | string | خیر | رزولوشن — پیش‌فرض "720x1280" |
| `input_reference` | file | خیر | عکس مرجع برای تولید ویدیو (multipart/form-data) |

**اندازه‌های معتبر:**
- Sora 2: `720x1280` (portrait)، `1280x720` (landscape)
- Sora 2 Pro: `720x1280`، `1280x720`، `1024x1792`، `1792x1024`
- Veo 3.1: `720x1280`، `1280x720`، `1080x1920`، `1920x1080`

**Response (بلافاصله بعد از submit):**
```json
{
  "id": "vid_abc123",
  "object": "video",
  "status": "queued",
  "model": "sora-2",
  "prompt": "...",
  "size": "1280x720",
  "seconds": 4,
  "created_at": "1763419001"
}
```

### بررسی وضعیت ویدیو

```
GET https://api.avalai.ir/v1/videos/{video_id}
Authorization: Bearer $KEY
```

**وضعیت‌های ممکن:** `queued` → `processing` → `completed` / `failed`

**Response:**
```json
{
  "id": "vid_abc123",
  "status": "completed",
  "progress": 100,
  "completed_at": "1763419063",
  "expires_at": 1766519691
}
```

> ⚠️ ویدیو منقضی می‌شود! `expires_at` دارد — باید قبل از انقضا دانلود و ذخیره شود.

### دانلود فایل ویدیو

```
GET https://api.avalai.ir/v1/videos/{video_id}/content
Authorization: Bearer $KEY
```

→ فایل binary ویدیو برمی‌گردد (mp4). باید دانلود و روی سرور ذخیره شود.

### لیست ویدیوها

```
GET https://api.avalai.ir/v1/videos
Authorization: Bearer $KEY
```

---

## Audio — Text to Speech (TTS)

### Endpoint

```
POST https://api.avalai.ir/v1/audio/speech
Content-Type: application/json
Authorization: Bearer $KEY
```

> ⚠️ Response یک فایل binary صوتی است (مثلاً mp3) — **نه JSON با URL**

**Request Body:**
```json
{
  "model": "tts-1",
  "input": "متن مورد نظر برای تبدیل به صدا",
  "voice": "nova"
}
```

| پارامتر | نوع | اجباری | توضیح |
|---|---|---|---|
| `model` | string | بله | ID مدل TTS |
| `input` | string | بله | متن ورودی — حداکثر 4096 کاراکتر |
| `voice` | string | بله | صدای مورد نظر |
| `response_format` | string | خیر | فرمت: `mp3` (پیش‌فرض)، `opus`، `aac`، `flac`، `wav`، `pcm` |
| `speed` | number | خیر | سرعت 0.25 تا 4.0 — پیش‌فرض 1.0 |

**مدل‌های TTS:**

| Model ID | توضیح |
|---|---|
| `tts-1` | استاندارد OpenAI |
| `tts-1-hd` | کیفیت بالاتر OpenAI |
| `gpt-4o-mini-tts` | GPT-4o mini TTS |
| `eleven_v3` | ElevenLabs v3 |
| `eleven_multilingual_v2` | ElevenLabs چندزبانه |
| `eleven_turbo_v2` | ElevenLabs سریع |
| `eleven_turbo_v2_5` | ElevenLabs سریع v2.5 |
| `eleven_flash_v2` | ElevenLabs Flash |
| `eleven_flash_v2_5` | ElevenLabs Flash v2.5 |
| `gemini-2.5-pro-preview-tts` | Google Gemini Pro TTS |
| `gemini-2.5-flash-preview-tts` | Google Gemini Flash TTS |
| `groq.playai-tts` | PlayAI از طریق Groq |
| `groq.playai-tts-arabic` | PlayAI فارسی/عربی از طریق Groq |

**صداهای پشتیبانی‌شده:** `alloy`، `ash`، `ballad`، `coral`، `echo`، `fable`، `onyx`، `nova`، `sage`، `shimmer`، `verse`

**Response:** فایل binary صوتی (Content-Type: `audio/mpeg` یا مشابه)

---

## Audio — Speech to Text (STT)

### Endpoint

```
POST https://api.avalai.ir/v1/audio/transcriptions
Content-Type: multipart/form-data
Authorization: Bearer $KEY
```

**Request:**
```bash
curl -F file="@audio.mp3" -F model="whisper-1"
```

**مدل‌های STT:** `whisper-1`، `gpt-4o-transcribe`، `groq.whisper-large-v3`، `groq.whisper-large-v3-turbo`

**Response:**
```json
{ "text": "متن transcribe شده..." }
```

---

## نکات پیاده‌سازی برای PromptMarketPlace

### ویدیو
1. `POST /videos` → دریافت `id` + `status: "queued"`
2. هر چند ثانیه `GET /videos/{id}` → بررسی `status`
3. وقتی `status == "completed"`: دانلود از `GET /videos/{id}/content`
4. ذخیره binary روی سرور (چون `expires_at` دارد)
5. jobId را با prefix `avalai:` ذخیره کن تا در polling تشخیص داده شود

### صدا
1. `POST /audio/speech` → Response binary است نه JSON
2. باید `Content-Type` header پاسخ را بررسی کرد
3. اگر `audio/*` بود → `ReadAsByteArrayAsync()` و ذخیره روی سرور
4. نیاز به `SaveBytesAsync` در `IStorageService`

### تفاوت با ChatQT
| موضوع | ChatQT | AvalAI |
|---|---|---|
| Image | `/chat/completions` → `message.images[0]` | `/images/generations` → `data[0].url` |
| Video | پشتیبانی ندارد | `/videos` (async) |
| Audio TTS | پشتیبانی ندارد | `/audio/speech` (binary response) |
| قیمت | دلاری | تومانی |

---

## تشخیص Provider در کد

چون نمی‌خواهیم baseUrl را hardcode کنیم، از ترتیب fallback استفاده می‌کنیم:

**ویدیو:**
1. `POST video/generations` (OpenAI standard)
2. اگر 4xx → `POST videos` (AvalAI)
3. اگر باز 4xx → `POST chat/completions` (ChatQT)

**JobId:** برای AvalAI با `avalai:` prefix ذخیره می‌شود تا در `CheckVideoStatusAsync` تشخیص داده شود.
