## ADDED Requirements

### Requirement: Admin Credit Pricing Page
The admin area SHALL provide a dedicated pricing page at `/Admin/Credits/Pricing` where admins can configure credit costs per output type using a formula-based calculator.

#### Scenario: Pricing formula displayed
- **WHEN** admin opens the pricing page
- **THEN** the full pricing formula is shown: `base_cost_usd × usd_rate × (1+VAT%) × (1+margin%) ÷ credit_value_irr`

#### Scenario: Model cost table
- **WHEN** AI models have `CostPer1KTokens`, `CostPerImage`, or `CostPerSecondVideo` set
- **THEN** the page shows estimated credit cost per model based on current pricing factors

#### Scenario: Pricing factors editable
- **WHEN** admin updates USD rate, VAT%, margin%, credit value, avg tokens, or avg video seconds
- **THEN** values are saved to `SystemSettings` under the `Pricing` group

### Requirement: Per Output-Type Credit Costs
Admin SHALL set final credit costs independently for Text/Code/Form, Image, Video, and Audio output types stored in SystemSettings.

#### Scenario: Credit cost saved per type
- **WHEN** admin saves pricing page
- **THEN** `Pricing:TextCreditCost`, `Pricing:ImageCreditCost`, `Pricing:VideoCreditCost`, `Pricing:AudioCreditCost` are persisted in SystemSettings

### Requirement: Apply Model to All Apps
The AI Models admin page SHALL include an "اعمال به همه ابزارها" button per model that bulk-updates all apps to use that model.

#### Scenario: Bulk model assignment
- **WHEN** admin clicks the globe icon button next to a model and confirms
- **THEN** all `Apps.AiModelId` records are updated to the selected model ID
- **AND** a success message shows how many apps were updated

### Requirement: External Pricing Reference Links
The pricing page SHALL display links to reference sources for updating pricing factors.

#### Scenario: Links visible
- **WHEN** admin views the pricing page
- **THEN** links to tgju.org, bonbast.com for USD rate and openrouter.ai/models for AI costs are shown
