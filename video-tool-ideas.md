# ۵ ایده ابزار ویدیویی پریمیوم

> تحقیق از: PromptBase، PromptHero، CloudPano، Mintly، Genra.ai — خرداد ۱۴۰۵
> فرمت: طبق ساختار ابزارهای موجود در سیستم (Creator Edit)

---

## ابزار ۱ — ساخت تبلیغ UGC برای محصولات فروشگاهی

### اطلاعات کلی

| فیلد | مقدار |
|---|---|
| **عنوان** | ساخت تبلیغ UGC محصول |
| **توضیح کوتاه** | یک ویدیوی تبلیغاتی اصیل شبیه محتوای کاربران واقعی برای محصول شما بساز |
| **نوع خروجی** | Video |
| **هزینه اعتباری پیشنهادی** | 20 اعتبار |

### توضیح کامل (برای صفحه ابزار)

تبلیغات ویدیویی UGC (محتوای کاربر-ساخته) ۳ تا ۵ برابر بیشتر از تبلیغات حرفه‌ای در تیک‌تاک و اینستاگرام کلیک و خرید می‌گیرند — چون واقعی به نظر می‌رسند.

این ابزار یک ویدیوی ۸ تا ۱۵ ثانیه‌ای می‌سازد که شبیه یک ویدیوی موبایلی اصیل است: دوربین دستی، نور طبیعی، فضای خانگی، و ریتم تیک‌تاکی. محصول شما در فریم اول دیده می‌شود و یک هوک جذاب در ۳ ثانیه اول.

استخدام یک سازنده UGC واقعی ۵۰۰ تا ۲۰۰۰ هزار تومان هزینه دارد. این ابزار همان نتیجه را در ۲ دقیقه می‌دهد.

---

### متن سیستمی (System Context)

```
You are a world-class UGC (user-generated content) video director specializing in viral TikTok and Instagram Reels product ads. You create authentic-feeling, high-converting short video ads that feel like real customer testimonials, not polished commercials.

Your videos:
- Feel genuinely filmed by a real person on a phone
- Have a scroll-stopping hook in the FIRST 3 seconds
- Show the product naturally, not as a catalog image
- Include implied social proof and authentic enthusiasm
- Use deliberate "imperfections": slight camera movement, natural room ambiance, casual framing

Key technique: The "Authenticity Stack" — layer handheld feel + natural light + casual setting + visible emotion to signal real UGC.

CRITICAL: No voiceover. No text overlays. Pure visual storytelling. Vertical 9:16 format.
Output: A single, detailed, cinematic Veo prompt. No explanation. Just the prompt.
```

### قالب پرامپت (Prompt Template)

```
Create a UGC-style product advertisement video for the following:

Product: {product_name}
Product type: {product_category}
Target audience: {target_audience}
Desired feeling/tone: {video_tone}

Requirements:
- Handheld iPhone-style vertical video (9:16)
- Person naturally holding/using the product in a {setting} setting
- Authentic, enthusiastic but casual body language
- Warm natural window light, soft ambient room glow
- Slight camera wobble to feel real, not staged
- Product clearly visible in the first 2 seconds
- Duration: [seconds:8][aspect:9:16]
- No text, no voiceover — pure visual
- Style: contemporary TikTok UGC, candid, warm color grade
```

### فیلدها

| نام | برچسب | نوع | پلیس‌هولدر / گزینه‌ها |
|---|---|---|---|
| `product_name` | نام محصول | Text | مثال: سرم ضد لک پوست |
| `product_category` | دسته‌بندی | Select | لوازم آرایشی / پوشاک / لوازم خانگی / غذا و نوشیدنی / دیجیتال / سایر |
| `target_audience` | مخاطب هدف | Text | مثال: زنان ۲۵ تا ۳۵ ساله علاقه‌مند به اسکین‌کر |
| `video_tone` | لحن ویدیو | Select | هیجان‌زده و شاد / آرام و اعتمادساز / کنجکاو و کاشف / قبل-بعد دراماتیک |
| `setting` | فضای فیلمبرداری | Select | خانه و آشپزخانه / اتاق خواب / فضای باز / کافه |

---

## ابزار ۲ — ویدیوی سینماتیک معرفی ملک و آپارتمان

### اطلاعات کلی

| فیلد | مقدار |
|---|---|
| **عنوان** | تور سینماتیک ملک |
| **توضیح کوتاه** | از توضیح ملکت یک ویدیوی حرفه‌ای cinematic بساز که خریدار را جذب کند |
| **نوع خروجی** | Video |
| **هزینه اعتباری پیشنهادی** | 20 اعتبار |

### توضیح کامل (برای صفحه ابزار)

آگهی‌های ملکی با ویدیو تا ۳۲٪ سریع‌تر به نتیجه می‌رسند. اما فیلمبرداری حرفه‌ای برای هر ملک ۵ تا ۲۵ میلیون تومان هزینه دارد.

این ابزار یک ویدیوی سینماتیک ۸ ثانیه‌ای از ملک شما می‌سازد: نمای بیرونی کرین‌شات، حرکت آرام دوربین در فضاهای داخلی، نور طلایی عصرگاهی، و حس لاکچری. نتیجه مثل ویدیوی یک تیم فیلمبرداری حرفه‌ای است.

### متن سیستمی

```
You are a luxury real estate videographer and AI prompt specialist. You create cinematic property walkthrough videos that make buyers fall in love with a property before they ever visit.

Your videos evoke the lifestyle the buyer dreams of — not just the physical space. You use:
- Drone-style aerial establishing shots
- Smooth dolly movements through spaces
- Golden hour lighting (the property's best version)
- Shallow depth of field emphasizing premium finishes
- Architectural photography principles: leading lines, framing, light play

Visual language: think architectural digest, not real estate listing.
Output: One precise Veo video prompt. No explanation. Just the prompt.
```

### قالب پرامپت

```
Create a cinematic real estate walkthrough video:

Property: {property_type} in {location}
Key features: {key_features}
Target buyer: {target_buyer}
Style: {visual_style}

Shot description:
- Opening: Slow aerial drone pull-back revealing the {property_type} exterior, golden hour warm light
- Middle: Smooth dolly shot gliding through the main living space, warm ambient light, large windows visible
- Reveal: Camera drifts toward the {highlight_feature}, soft focus foreground element
- Lighting: Warm late-afternoon golden light, {visual_style} color grade
- Camera: Cinematic 24fps, shallow depth of field f/2.8 equivalent, 4K crisp
- Duration: [seconds:8][aspect:16:9]
- No people, no text, no voiceover
```

### فیلدها

| نام | برچسب | نوع | پلیس‌هولدر / گزینه‌ها |
|---|---|---|---|
| `property_type` | نوع ملک | Select | آپارتمان لاکچری / ویلا / خانه مسکونی / دفتر تجاری / استودیو |
| `location` | موقعیت | Text | مثال: شمال تهران، کنار دریا، منطقه مرکزی شهر |
| `key_features` | ویژگی‌های اصلی | Textarea | مثال: پنجره‌های بزرگ با منظره، آشپزخانه اوپن، استخر روی پشت‌بام |
| `target_buyer` | خریدار هدف | Select | خانواده جوان / سرمایه‌گذار / مجرد حرفه‌ای / توریست (Airbnb) |
| `visual_style` | سبک تصویری | Select | گرم و دنج / مدرن و مینیمال / لاکچری طلایی / روشن و اسکاندیناوی |
| `highlight_feature` | نقطه فروش اصلی | Text | مثال: منظره شهر، استخر، تراس بزرگ |

---

## ابزار ۳ — ویدیوی تبلیغاتی برای رستوران و کافه

### اطلاعات کلی

| فیلد | مقدار |
|---|---|
| **عنوان** | ویدیو تبلیغاتی غذا و نوشیدنی |
| **توضیح کوتاه** | یک ویدیوی ماکرو اشتها‌آور از غذا یا نوشیدنی‌ات بساز که فروش آنلاینت را چند برابر کند |
| **نوع خروجی** | Video |
| **هزینه اعتباری پیشنهادی** | 20 اعتبار |

### توضیح کامل

تصاویر حرفه‌ای غذا فروش آنلاین را تا ۴۰٪ بالا می‌برند. ویدیوی غذایی که بخار داره، پنیر کش میاد، یا قهوه ریخته میشه — این‌ها ناخودآگاه مشتری را وادار به سفارش می‌کنند.

این ابزار یک ویدیوی ۸ ثانیه‌ای با کیفیت استودیوی عکاسی غذا می‌سازد: ماکرو لنز نزدیک، نور حرفه‌ای، حرکت آرام دوربین، و جزئیاتی که می‌چرخد معده.

### متن سیستمی

```
You are a professional food videographer and sensory storyteller. You create mouth-watering food and beverage videos that trigger hunger and desire using cinematic techniques.

Your videos are built on "sensory layering":
- VISUAL: extreme macro close-ups, steam, melts, drips, pours, cheese pulls
- LIGHT: warm studio food photography lighting, soft key light, rim light on steam
- MOTION: ultra-slow motion (implied), smooth tilt-up reveals, gentle rotation
- COLOR: rich, saturated food colors, warm amber tones, no artificial plastic look

The 3-second rule: the most appetizing moment hits before second 3.
Output: One precise, highly sensory Veo prompt. Nothing else.
```

### قالب پرامپت

```
Create a cinematic, mouth-watering food/beverage advertisement video:

Dish/drink: {food_name}
Cuisine/category: {cuisine_type}
Restaurant vibe: {restaurant_vibe}
Hero visual moment: {hero_moment}

Shot:
- Extreme macro close-up of {food_name}, {hero_moment} in ultra-slow motion
- Warm professional food photography lighting: soft key light, golden rim light catching steam
- Camera: gentle slow tilt-up reveal OR slow orbit around the dish
- Background: slightly blurred {restaurant_vibe} ambiance, bokeh lights
- Color grade: {cuisine_type} appropriate — warm amber for comfort food, cool clean for modern cuisine
- No people visible, no text, product-focused
- Duration: [seconds:8][aspect:1:1]
- Style: commercial food photography, Michelin-level presentation
```

### فیلدها

| نام | برچسب | نوع | پلیس‌هولدر / گزینه‌ها |
|---|---|---|---|
| `food_name` | نام غذا یا نوشیدنی | Text | مثال: پیتزا مارگاریتا، کاپوچینو، برگر دابل |
| `cuisine_type` | نوع آشپزی | Select | ایرانی / ایتالیایی / فست‌فود / قهوه و کافه / ژاپنی / شیرینی و دسر |
| `restaurant_vibe` | فضای رستوران | Select | کافه دنج / رستوران لاکچری / فست‌فود شاد / سنتی ایرانی |
| `hero_moment` | لحظه اشتها‌آور | Select | کشیده شدن پنیر / بخار داغ / ریختن سس / قطره قهوه / کات اول چاقو |

---

## ابزار ۴ — ویدیوی اکسپلینر برای اپ و استارتاپ

### اطلاعات کلی

| فیلد | مقدار |
|---|---|
| **عنوان** | ویدیوی معرفی محصول و استارتاپ |
| **توضیح کوتاه** | یک ویدیوی اکسپلینر حرفه‌ای برای صفحه اصلی یا پیچ‌دکت استارتاپت بساز |
| **نوع خروجی** | Video |
| **هزینه اعتباری پیشنهادی** | 20 اعتبار |

### توضیح کامل

ویدیوهای اکسپلینر در صفحه لندینگ نرخ تبدیل را ۲۰ تا ۸۰٪ بالا می‌برند. هر استارتاپ به یکی نیاز دارد — اما استودیوهای تخصصی ۳ تا ۱۵ هزار دلار می‌گیرند.

این ابزار یک ویدیوی ۸ ثانیه‌ای از بخش مشکل یا راه‌حل محصولت می‌سازد: پرزنتر اعتمادساز، محیط مدرن، و حس SaaS حرفه‌ای.

### متن سیستمی

```
You are a B2B video producer specializing in SaaS and startup explainer videos. Your videos communicate trust, innovation, and clear value proposition in under 10 seconds.

Visual language:
- Clean, modern environments: glass offices, minimalist desks, tech-forward backgrounds
- Confident, professional presenter or product-focused abstract visualization
- Motion: smooth camera movements suggesting progress and clarity
- Color: crisp whites, deep blues/purples, accent colors suggesting innovation
- Lighting: clean studio lighting or bright natural office light — authoritative feel

Structure for 8 seconds: Problem hint (2s) → Solution reveal (4s) → Result/CTA (2s)
Output: One focused Veo prompt for one key scene. No explanation.
```

### قالب پرامپت

```
Create a professional startup/SaaS explainer video scene:

Product name: {product_name}
What it does (one line): {product_description}
Target customer: {target_customer}
Scene to visualize: {scene_focus}
Visual style: {visual_style}

Video:
- Scene: {scene_focus} — showing {product_name} in use or the problem it solves
- Setting: Modern {visual_style} environment — clean, bright, professional
- Subject: {target_customer} persona, confident, clearly focused on a screen/device
- Camera: Smooth slow push-in or gentle orbit, giving a sense of discovery
- Lighting: Bright natural office light, clean shadows, product clearly visible
- Color grade: Corporate-tech palette — crisp whites, deep accent on {visual_style} tones
- Duration: [seconds:8][aspect:16:9]
- No text overlays, no voiceover
```

### فیلدها

| نام | برچسب | نوع | پلیس‌هولدر / گزینه‌ها |
|---|---|---|---|
| `product_name` | نام محصول | Text | مثال: TaskFlow AI |
| `product_description` | توضیح یک‌خطی | Text | مثال: مدیریت پروژه با هوش مصنوعی |
| `target_customer` | مشتری هدف | Select | فریلنسر / تیم استارتاپ / مدیر میانی / کارآفرین |
| `scene_focus` | سکانس موردنظر | Select | لحظه آها (مشکل حل می‌شود) / پرزنتر معرفی می‌کند / داشبورد محصول / تیم همکاری |
| `visual_style` | سبک بصری | Select | مینیمال سفید / تاریک تکنولوژیک / گرم و انسانی / آبی کورپوریت |

---

## ابزار ۵ — ویدیوی مد و جواهرات لاکچری

### اطلاعات کلی

| فیلد | مقدار |
|---|---|
| **عنوان** | ویدیوی اتمسفریک برند مد و جواهر |
| **توضیح کوتاه** | یک ویدیوی سینماتیک لاکچری از محصول مدت با حس ادیتوریال Vogue بساز |
| **نوع خروجی** | Video |
| **هزینه اعتباری پیشنهادی** | 20 اعتبار |

### توضیح کامل

برندهای مد و جواهرات کوچک هر هفته به محتوای ویدیویی با کیفیت ادیتوریال نیاز دارند — اما یک شوت حرفه‌ای فشن ۵۰ تا ۵۰۰ میلیون تومان هزینه دارد.

این ابزار یک ویدیوی ۸ ثانیه‌ای با استتیک Dior/Chanel می‌سازد: حرکت آهسته پارچه، نور طلایی بر جواهر، گرین فیلم آنالوگ. خروجی‌ای که هیچ‌کس باور نمی‌کند با هوش مصنوعی ساخته شده.

### متن سیستمی

```
You are a luxury fashion film director, trained in the aesthetic language of Dior, Chanel, Hermès, and high fashion editorial photography. You create atmospheric, aspirational video content that sells a feeling, not a product.

Your visual vocabulary:
- Film grain: Kodak Vision3 250D feel — lifted shadows, warm cast, slight desaturation
- Light: Golden hour raking light catching fabric texture, jewelry sparkle, material depth
- Motion: Extreme slow motion implied — fabric ripple, hair movement, watch reflection
- Depth: f/1.4 equivalent, foreground bokeh, product sharp, background painterly
- Mood: Timeless, European, romantic, aspirational — never commercial, never catalog

Forbidden: flat lighting, plastic feel, obvious AI artifacts, busy backgrounds, commercial composition.
Output: One ultra-precise Veo prompt using sensory and aesthetic language. Nothing else.
```

### قالب پرامپت

```
Create a luxury fashion/jewelry atmospheric video:

Product: {product_name} ({product_type})
Brand aesthetic: {brand_aesthetic}
Setting: {setting_choice}
Target platform: {platform}

Video:
- Subject: {product_name}, a {product_type} — shown in intimate detail
- Moment: {hero_visual} in ultra-slow motion, light catching every detail
- Setting: {setting_choice} — soft, diffused light, painterly background
- Camera: Extreme close-up drifting gently, shallow DOF, foreground element softly blurred
- Film look: {brand_aesthetic} color grade — Kodak film scan, lifted blacks, warm cast OR cool silver tones
- Mood: Timeless, aspirational, tactile — the viewer wants to reach into the screen and touch it
- Duration: [seconds:8][aspect:{platform}]
- No people (product-only), no text, no branding
```

### فیلدها

| نام | برچسب | نوع | پلیس‌هولدر / گزینه‌ها |
|---|---|---|---|
| `product_name` | نام محصول | Text | مثال: گردنبند طلای مینیمال |
| `product_type` | نوع محصول | Select | جواهرات / کیف و کفش / پوشاک و پارچه / ساعت / عطر (بدون محصول) |
| `brand_aesthetic` | زبان بصری برند | Select | پاریسی کلاسیک / اسکاندیناوی مینیمال / شرقی مدرن / طلایی باشکوه / سیاه‌وسفید ابدی |
| `setting_choice` | فضای فیلم | Select | فضای باز طبیعت / استودیو سفید مینیمال / اتاق قدیمی اروپایی / ساحل و آب |
| `hero_visual` | لحظه کلیدی بصری | Select | انعکاس نور روی جواهر / حرکت پارچه در باد / باز شدن قوطی جواهر / افتادن قطره آب روی محصول |
| `platform` | پلتفرم هدف | Select | اینستاگرام ریلز (9:16) / استوری (9:16) / پینترست (1:1) / یوتیوب (16:9) |

---

## خلاصه مقایسه‌ای

| # | ابزار | مخاطب | ارزش جایگزین‌شده | اعتبار |
|---|---|---|---|---|
| ۱ | تبلیغ UGC | فروشگاه‌های آنلاین | ۵۰۰–۲۰۰۰ هزار تومان/ویدیو | ۲۰ |
| ۲ | تور ملک | مشاوران مسکن | ۵–۲۵ میلیون/لیستینگ | ۲۰ |
| ۳ | تبلیغ غذا | رستوران و کافه | ۳–۱۰ میلیون/شوت | ۲۰ |
| ۴ | اکسپلینر استارتاپ | بنیان‌گذاران | ۱۵–۷۵ میلیون/ویدیو | ۲۰ |
| ۵ | مد و جواهر لاکچری | برندهای فشن | ۵۰–۵۰۰ میلیون/شوت | ۲۰ |
