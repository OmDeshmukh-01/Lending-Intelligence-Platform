# AI Interaction Log
I have created the log based on the steps taken by me to build the entire project i have added the stage by stage prompts ai suggestions and the my review and updation in codebase 

This log covers how I used AI tools (primarily Google Gemini / ChatGPT) to speed up the development of this lending platform test. I used it mostly for scaffolding, boilerplate generation, and checking edge cases, while keeping tight control over the actual domain logic and architecture.

## 1. Requirements Analysis
Before writing any code, I wanted to make sure I had the business rules mapped out perfectly. 

**My Prompt:**
> Analyse the lending platform technical test below. Do not write code yet. Extract all explicit business rules, input validations, output requirements, boundary conditions, ambiguities, and potential edge cases. Convert the lending rules into a clear decision table that can later be implemented and tested.

**What the AI generated:**
It gave me a solid decision table breaking down the LTV bands (<60%, <80%, <90%) and the credit score requirements. 

**My Review & Action:**
The table looked correct. It correctly identified that the bands were exclusive (e.g., LTV < 80% inherently assumes LTV >= 60% based on the previous rule). I used this table as the blueprint for my `LoanEvaluator` class.

## 2. Identifying Ambiguities
I always like to double-check requirements for vague wording before committing to an architecture.

**My Prompt:**
> Review the requirements and identify any statements that could have multiple interpretations. Pay particular attention to boundary values such as £100,000, £1 million, 60% LTV, 80% LTV, 90% LTV, and how mean LTV should be calculated. Do not make assumptions silently; list the ambiguity and possible interpretations.

**What the AI generated:**
It caught a great edge case: The spec asks for the "Mean average Loan to Value (LTV) across all applications." The AI pointed out this could mean *only successful* applications, or *literally all* (successful and declined) applications.

**My Review & Action:**
I decided to interpret "all applications" literally. In my implementation, the Mean LTV calculation on the dashboard includes both approved and declined applications.

## 3. Architecture Design
I wanted to set up a Clean Architecture project quickly without manually creating all the boilerplate folders.

**My Prompt:**
> Based on this lending platform specification, propose a clean architecture for a C# ASP.NET Core Web API and React frontend. Focus on separation of concerns, testability, maintainability and keeping business rules independent from controllers and UI. Do not over-engineer the solution.

**What the AI generated:**
It suggested a classic N-tier structure: Frontend -> API Controllers -> Application Layer -> Domain Rules -> Repository -> SQLite Database.

**My Review & Action:**
I agreed with this approach. I specifically chose to keep the lending decision logic isolated in a pure C# Domain service (`LoanEvaluator.cs`) rather than dumping it into the API controllers. This kept the API layer focused strictly on HTTP requests and made the business rules completely independently testable.

## 4. Business Logic Implementation
Once the skeleton was there, I asked the AI to take a stab at the core lending logic.

**My Prompt:**
> Implement the lending decision rules as a clean C# service. Keep the business logic independent from ASP.NET controllers and database code. Handle the exact boundary conditions specified in the requirements. Explain the reasoning behind the ordering of the rules before providing the implementation.

**What the AI generated:**
It generated a series of `if/else` statements for the £1M threshold and the LTV bands.

**My Review & Action:**
I didn't just blindly accept it. I followed up by asking: 
> "Review your previous implementation specifically for logical overlap between the LTV ranges. Give examples of inputs that could be incorrectly classified because of rule ordering."

The AI pointed out that if we weren't careful with `>=` and `<`, a loan with exactly 60.00% LTV might slip into the wrong band. Based on this, I manually refactored the C# implementation to explicitly define and handle the exact boundary constraints (`Band1MaxLtv = 60m`, etc.) to prevent any edge-case bleed.

## 5. Challenging the Business Logic
To prove the logic was bulletproof, I needed test cases.

**My Prompt:**
> Generate a comprehensive set of boundary and edge-case test cases for this lending decision engine. Include values exactly at £100,000, £1,000,000, £1,500,000, LTV exactly at 60%, 80%, and 90%, and credit scores exactly at 750, 800, 900 and 950. For each case, provide the expected decision and explain which rule applies.

**What the AI generated:**
It gave me a great list of edge cases, such as:
- Loan = £1,000,000, LTV = 60%, Score = 950 -> Expected: Approved
- Loan = £1,000,000, LTV = 60.01%, Score = 999 -> Expected: Declined

**My Review & Action:**
I manually verified its math and logic against the spec, and then converted these exact scenarios into data-driven xUnit `[Theory]` tests in the backend. 

## 6. API Design
**My Prompt:**
> Suggest REST API endpoints for submitting a loan application and retrieving lending statistics. Keep the API minimal and aligned with the requirements. Explain what responsibility belongs in the controller versus the service layer.

**What the AI generated:**
It suggested a minimal API: `POST /api/loans` for submissions and `GET /api/dashboard` for the stats.

**My Review & Action:**
Looks good. I made sure the controller only handled HTTP routing (returning 400 for bad input and 200 for successful processing, even if the loan itself was declined) and pushed the actual orchestration down to the Application layer.

## 7. Frontend Design
I used AI to rapidly scaffold the React components using Tailwind.

**My Prompt:**
> Design a simple React UI for this lending platform. The application should have a sidebar containing Dashboard, Previous Applications and New Application. The dashboard should display application statistics and the five most recent applications. The new application page should collect loan amount, asset value and credit score and display the lending decision after submission. Keep the UI clean and suitable for a technical assessment.

**What the AI generated:**
It built a nice layout but the initial dashboard was a bit text-heavy with basic tables.

**My Review & Action:**
I iterated on the design. I asked the AI to add Recharts to visualize the accepted/declined ratio as a pie chart, and the loan value distribution as a bar chart. When the AI initially created 15 tiny, overlapping buckets for the bar chart (£100k increments), I intervened and had it rewrite the backend query to group them into 4 readable buckets (£100k-499k, £500k-999k, etc.). This made the UI much cleaner.

## 8. Integration Testing
**My Prompt:**
> Review the API/frontend integration flow and identify potential problems involving validation, API errors, loading states, empty application history, and inconsistent statistics.

**What the AI generated:**
It reminded me that floating-point math in JavaScript (and SQLite) could cause LTV rounding issues, meaning the frontend might calculate an LTV slightly differently than the C# backend.

**My Review & Action:**
To avoid this, I designed the API so the frontend *never* calculates LTV itself. It only displays the exact `decimal` LTV returned by the C# backend. I also configured EF Core to store decimals as `TEXT` in SQLite to prevent precision loss at the database level.

## 9. AI Code Review
Once the core was done, I asked the AI to audit my work.

**My Prompt:**
> Act as a senior C# reviewer. Review this lending platform implementation against the following evaluation criteria: Correctness, Clarity, Separation of concerns, Error handling, and Testability. Do not rewrite the code immediately. First identify concrete issues and explain why each issue matters.

**What the AI generated & My Decisions:**
- **Move business rules out of controller:** Accepted. Kept strictly in the Domain layer.
- **Add repository abstraction:** Accepted. Created `ILoanRepository` to abstract SQLite.
- **Add complex CQRS (MediatR) architecture:** Rejected. This would be massive over-engineering for a simple technical test, violating the prompt's instructions to keep it simple.
- **Add unit tests for boundary cases:** Accepted. 

Rejecting the CQRS suggestion was important because blindly applying enterprise patterns to small tests usually does more harm than good.

## 10. Final Review
**My Prompt:**
> Perform a final review of this lending platform against the original technical test. Check the implementation against every explicit requirement and identify any missing functionality, incorrect business logic, questionable assumptions, unnecessary complexity, or documentation gaps. Do not modify the code.

**What the AI generated:**
It did a final pass and caught one subtle thing: The requirement asks for the "Total value of loans written to date". It pointed out this should strictly only sum the `LoanAmount` of *Successful* applications, not declined ones.

**My Review & Action:**
I double-checked my LINQ query in `LoanRepository.cs` and confirmed that I was already filtering correctly (`successful.Sum(x => x.LoanAmount)`). The implementation was completely ready to go.
