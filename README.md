# AI-Driven Helpdesk (ADH)

AI-Driven Helpdesk (ADH) is an advanced, intelligent support system designed to modernize IT operations. It combines the power of Large Language Models (LLMs) with proactive infrastructure management and robust security protocols to provide a comprehensive solution for corporate environments.

## The Problem ADH Solves

Traditional helpdesk operations are often bogged down by repetitive manual tasks, reactive maintenance, and privacy concerns when integrating modern AI tools. ADH solves several critical challenges:

*   **High Operational Overhead:** Automates routine support queries and ticket classifications using AI, freeing up IT staff for complex tasks.
*   **Privacy & Data Security:** External AI services pose a risk to sensitive corporate data. ADH includes an automated **PII (Personally Identifiable Information) Scrubber** that sanitizes queries before they reach the AI, and supports **Sovereign AI** (local LLMs via Ollama) for air-gapped environments.
*   **Reactive Infrastructure Management:** Instead of waiting for systems to fail, ADH features **Self-Healing** capabilities that automatically monitor disk space, restart services, and clear temporary files.
*   **Manual Asset Tracking:** Maintains an up-to-date **Asset Inventory (CMDB)** by automatically discovering hardware through network scanning (Ping) and Active Directory integration.
*   **Slow Response Times:** Provides real-time assistance via an AI-driven chat interface, ensuring users get immediate help for common issues.

## Key Features

*   **Intelligent Chat Assistant:** A real-time chat interface powered by Semantic Kernel, supporting streaming responses and SignalR for live status updates (e.g., "AI is typing...").
*   **Privacy-First AI:** Integrated PII scrubbing to protect user data.
*   **Infrastructure Autonomy:** Proactive self-healing logic to maintain system health without manual intervention.
*   **Automated Asset Discovery:** Scans local networks and integrates with LDAP/Active Directory to track corporate assets automatically.
*   **Data Sovereignty:** Fully compatible with local AI engines (Ollama), allowing the entire system to run in a private, offline network.
*   **Jira Integration:** Seamlessly connects with Jira for ticket management.

## Architecture & Technology Stack

*   **Backend:** .NET 10 (C#) following **Clean Architecture** principles.
*   **Frontend:** React with TypeScript, Tailwind CSS, and Shadcn UI.
*   **AI Orchestration:** Microsoft Semantic Kernel.
*   **Communication:** SignalR for real-time notifications.
*   **Database:** PostgreSQL with `pgvector` for semantic search.
*   **Containerization:** Docker & Docker Compose for easy deployment.

## Getting Started

### Prerequisites
*   Docker and Docker Compose
*   .NET 10 SDK (for local development)
*   Node.js & npm (for frontend development)

### Quick Start
1.  Clone the repository.
2.  Configure your environment variables in `.env` (use `.env.example` as a template).
3.  Run the system using Docker:
    ```bash
    docker-compose up --build
    ```
4.  Access the frontend at `http://localhost:3000` and the API documentation at `http://localhost:5000/swagger`.

## Security & Compliance

ADH is built with a "Security by Design" approach, featuring Prompt Injection protection, automated scrubbing of sensitive data, and full support for air-gapped deployment to meet the strictest corporate compliance requirements.
