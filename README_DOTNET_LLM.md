# Date: 2025/08/20, 2:00 PM

# General, Misc, Uncategorizable (Yet) Resources

## Resources / References

-   [Let's Learn .NET - Streaming Live Every Week](https://dotnet.microsoft.com/en-us/live/lets-learn-dotnet)
-   [Building AI Apps in .NET Just Got 10x Easier - Microsoft.Extensions.AI](https://www.youtube.com/watch?v=4B3ppx2U8bE)
-   [Ollama (Local LLM) Integration using Semantic Kernel](https://mehmetozkaya.medium.com/ollama-integration-using-semantic-kernel-and-c-a-step-by-step-guide-311b7d163b67)
-   [Semantic Search Demo with Local LLM](https://github.com/Giorgi/Semantic-Search-Demo)
-   [Local AI app in .NET](https://www.reddit.com/r/dotnet/comments/1ijqgqr/local_ai_app_in_net/)
    -   [Reddit User Guide for Local LLM App](https://www.reddit.com/r/dotnet/comments/1ijqgqr/comment/mbl3v6t/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button)
-   [Semantic Kernel Cookbook\*](https://github.com/kinfey/SemanticKernelCookBook/tree/main)
-   [Microsoft Guide - Generative AI for Beginners .NET\*](https://github.com/microsoft/Generative-AI-for-beginners-dotnet)
    -   Practical lessons and sample codes teaching you how to build Generative AI applications in .NET
-   [Azure Sample - Chat with your Data\*](https://aka.ms/netaichatwithyourdata)
    -   Chat with your Data is a reference .NET application implementing an Chat application based on documents content with search features using Semantic Search with Vector Database and Azure AI Search.
-   [Ollama Official Documentation](https://ollama.readthedocs.io/en/)
-   [LLamaSharp Documentation](https://scisharp.github.io/LLamaSharp/0.4/)
-   [RAG in C# using DeepSeek](https://youtu.be/KAZ4MJ9HPpU?si=vOTmbYHjaQIo_4sz)
-   [ONNX to run LLM](https://www.reddit.com/r/LocalLLaMA/comments/14q24n7/onnx_to_run_llm/)
-   [Using Phi-3 & C# with ONNX for text and vision samples](https://devblogs.microsoft.com/dotnet/using-phi3-csharp-with-onnx-for-text-and-vision-samples-md/)
-   [How to build docker image for python flask app](https://www.youtube.com/watch?v=0eMU23VyzR8)
-   [Deploy Python Applications - Google Cloud Run with Docker](https://www.youtube.com/watch?v=sqUuofLBfFw)
-   [Run LLM 100% Locally with Docker's New Model Runner](https://www.reddit.com/r/docker/comments/1k0tygf/run_llms_100_locally_with_dockers_new_model_runner/)
-   [The Easiest Ways to Run LLMs Locally without Olllam - Docker Model Runner Tutorial](https://www.youtube.com/watch?v=GOgfQxDPaDw)
-   [Build a Local AI Chatbot with Python, Flask & Ollama (Contained in Docker) – With Conversation & Memory Support!\*](https://www.youtube.com/watch?v=UxmqWy_1xuU)
-   [Self Hosted DeepSeek & ChatGPT Alternative (Ollama + Docker)](https://www.youtube.com/watch?v=94czqeE4jss)
-   [How to run Ollama on Docker](https://www.youtube.com/watch?v=ZoxJcPkjirs)
-   [What the Heck is MCP? (& what it isn't)](https://www.gettingstarted.ai/mcp-vs-rag-vs-api/)
-   [Run LLM inference on Cloud Run GPUs with Gemma 3 and Ollama](https://cloud.google.com/run/docs/tutorials/gpu-gemma-with-ollama)
-   [How to run (any) open LLM with Ollama on Google Cloud Run [Step-by-step]\*](https://geshan.com.np/blog/2025/01/ollama-google-cloud-run/)
-   [Run Serverless LLMs with Ollama and Cloud Run (GPU Support)](https://youtu.be/JhCWELvaQSU?si=z7vA-dxxIwvHIEW9)
    -   [ollama-cloud-run](https://github.com/geshan/ollama-cloud-run/tree/master)
-   [How to run any hugging face model with Ollama](https://www.youtube.com/watch?v=r-_xykTmz_o)

## Google Search Questions To Dig Deeper into

-   how to run local mini llm in ollama and have it get serve by a flask app both contain in a docker container and deploy on cloud run

# Semantic Kernal (.NET)

-   Semantic Kernel is an open source SDK that allows you to integrate large language models (LLMs) into your own code. Using the Semantic Kernel SDK, you can create intelligent applications respond and react to natural language prompts. The possibilities of artificially intelligent (AI) applications are endless, and the Semantic Kernel SDK can help you create AI agents that automate tasks, provide personalized recommendations, and more.

-   Suppose you're a developer for Margie's Travel, a global leader in the travel and hospitality industry. You're tasked with creating a personalized AI travel agent. Rather than creating your own language processing model from scratch, you can use the Semantic Kernel to interface with the language model of your choice and create an AI agent that can:

    -   Understand natural language.
    -   Provide customized recommendations.
    -   Book travel accommodations.
    -   And more!

This module introduces you to the Semantic Kernel SDK. You can learn how the kernel extends functionality by connecting code to LLMs. You can also learn how the SDK can be used to create artificially intelligent agents that can automate custom tasks.

-   The kernel acts as a Dependency Injection container that manages all the services and plugins required for your AI application. It facilitates the seamless interaction between your code, AI models, and external tools by providing a centralized point for managing these components. It's essentially the core engine that powers the framework, handling tasks, functions, and interactions.

## Kernel

-   The kernel is the central component of Semantic Kernel. At its simplest, the kernel is a Dependency Injection container that manages all of the services and plugins necessary to run your AI application. If you provide all of your services and plugins to the kernel, they will then be seamlessly used by the AI as needed.

-   Because the kernel has all of the services and plugins necessary to run both native code and AI services, it is used by nearly every component within the Semantic Kernel SDK to power your agents. This means that if you run any prompt or code in Semantic Kernel, the kernel will always be available to retrieve the necessary services and plugins.

-   This is extremely powerful, because it means you as a developer have a single place where you can configure, and most importantly monitor, your AI agents. Take for example, when you invoke a prompt from the kernel. When you do so, the kernel will...

    1. Select the best AI service to run the prompt.
    2. Build the prompt using the provided prompt template.
    3. Send the prompt to the AI service.
    4. Receive and parse the response.
    5. And finally return the response from the LLM to your application.

-   Throughout this entire process, you can create events and middleware that are triggered at each of these steps. This means you can perform actions like logging, provide status updates to users, and most importantly responsible AI. All from a single place.

## Services

-   These consist of both AI services (e.g., chat completion) and other services (e.g., logging and HTTP clients) that are necessary to run your application. This was modelled after the Service Provider pattern in .NET so that we could support dependency injection across all languages.

## Plugins

-   Plugins are the components that are used by your AI services and prompt templates to perform work. AI services, for example, can use plugins to retrieve data from a database or call an external API to perform actions.

    -   Plugins extend the kernel's capabilities by providing specific functionalities, while agents are intelligent orchestrators that leverage plugins to handle complex tasks and interact with LLMs.

-   Plugins are named function containers. Plugins are wrappers around functions or external services that can be invoked by the Semantic Kernel. Each can contain one or more functions. Plugins can be registered with the kernel, which allows the kernel to use them in two ways:

    1. Advertise them to the chat completion AI, so that the AI can choose them for invocation.
    2. Make them available to be called from a template during template rendering.

-   Functions can easily be created from many sources, including from native code, OpenAPI specs, `ITextSearch` implementations for RAG scenarios, but also from prompt templates.

-   Function calling is a powerful tool that allows developers to add custom functionalities and expand the capabilities of AI applications. The Semantic Kernel Plugin architecture offers a flexible framework to support Function Calling.

### Purpose

-   They allow developers to integrate custom logic, connect to external APIs, or utilize specialized tools within the AI application.

### Example

-   A plugin could be a function that calculates the area of a rectangle, a service that retrieves weather information, or a component that interacts with a database.

### Relation to Agents

-   For an Agent, integrating Plugins and Function Calling is built on this foundational Semantic Kernel feature.

-   Agents utilize plugins to perform tasks. For instance, an agent might use a "WeatherPlugin" to get the current temperature before making a decision

-   Once configured, an agent will choose when and how to call an available function, as it would in any usage outside of the Agent Framework.

## Prompt Template

-   Prompt templates allow a developer or prompt engineer to create a template that mixes context and instructions for the AI with user input and function output. E.g. the template may contain instructions for the Chat Completion AI model, and placeholders for user input, plus hardcoded calls to plugins that always need to be executed before invoking the Chat Completion AI model (LLM).

-   Prompt templates can be used in two ways:

    1. As the starting point of a Chat Completion flow by asking the kernel to render the template and invoke the Chat Completion AI model (LLM) with the rendered result.

    2. As a plugin function, so that it can be invoked in the same way as any other function can be.

-   When a prompt template is used, it will first be rendered, plus any hardcoded function references that it contains will be executed. The rendered prompt will then be passed to the Chat Completion AI model (LLM). The result generated by the AI will be returned to the caller.

    -   If the prompt template had been registered as a plugin function, the function may have been chosen for execution by the Chat Completion AI model and in this case the caller is Semantic Kernel, on behalf of the AI model.

-   Using prompt templates as plugin functions in this way can result in rather complex flows. E.g. consider the scenario where a prompt template A is registered as a plugin. At the same time a different prompt template `B` may be passed to the kernel to start the chat completion flow. `B` could have a hardcoded call to `A`. This would result in the following steps:

    1. `B` rendering starts and the prompt execution finds a reference to `A`
    2. `A` is rendered.
    3. The rendered output of `A` is passed to the Chat Completion AI model.
    4. The result of the Chat Completion AI model is returned to `B`.
    5. Rendering of `B` completes.
    6. The rendered output of `B` is passed to the Chat Completion AI model.
    7. The result of the Chat Completion AI model is returned to to the caller.

-   Also consider the scenario where there is no hardcoded call from B to A. If function calling is enabled, the Chat Completion AI model (LLM) may still decide that `A` should be invoked since it requires data or functionality that `A` can provide.

Registering prompt templates as plugin functions allows for the possibility of creating functionality that is described using human language instead of actual code. Separating the functionality into a plugin like this allows the AI model to reason about this separately to the main execution flow, and can lead to higher success rates by the AI model, since it can focus on a single problem at a time.

## Filters

-   Filters provide a way to take custom action before and after specific events during the chat completion flow. These events include:

    -   Before and after function invocation.
    -   Before and after prompt rendering.

-   Filters need to be registered with the kernel to get invoked during the chat completion flow.

-   Note that since prompt templates are always converted to KernelFunctions before execution, both function and prompt filters will be invoked for a prompt template. Since filters are nested when more than one is available, function filters are the outer filters and prompt filters are the inner filters.

## Vector Store (Memory) Connectors

-   The Semantic Kernel Vector Store connectors provide an abstraction layer that exposes vector stores from different providers via a common interface. The Kernel does not use any registered vector store automatically, but Vector Search can easily be exposed as a plugin to the Kernel in which case the plugin is made available to Prompt Templates and the Chat Completion AI Model.

## AI Service Connectors

-   The Semantic Kernel AI service connectors provide an abstraction layer that exposes multiple AI service types from different providers via a common interface. Supported services include Chat Completion, Text Generation, Embedding Generation, Text to Image, Image to Text, Text to Audio and Audio to Text.

-   When an implementation is registered with the Kernel, Chat Completion or Text Generation services will be used by default, by any method calls to the kernel. None of the other supported services will be used automatically.

-   One of the main features of Semantic Kernel is its ability to add different AI services to the kernel. This allows you to easily swap out different AI services to compare their performance and to leverage the best model for your needs.

## Agents

-   An AI agent is a software entity designed to perform tasks autonomously or semi-autonomously by receiving input, processing information, and taking actions to achieve specific goals.

-   Agents can send and receive messages, generating responses using a combination of models, tools, human inputs, or other customizable components.

-   Agents are designed to work collaboratively, enabling complex workflows by interacting with each other. The Agent Framework allows for the creation of both simple and sophisticated agents, enhancing modularity and ease of maintenance

### Problems an AI Agents Solve

-   AI agents offers several advantages for application development, particularly by enabling the creation of modular AI components that are able to collaborate to reduce manual intervention in complex tasks. AI agents can operate autonomously or semi-autonomously, making them powerful tools for a range of applications.

-   Here are some of the key benefits:

    -   Modular Components: Allows developers to define various types of agents for specific tasks (e.g., data scraping, API interaction, or natural language processing). This makes it easier to adapt the application as requirements evolve or new technologies emerge.

    -   Collaboration: Multiple agents may "collaborate" on tasks. For example, one agent might handle data collection while another analyzes it and yet another uses the results to make decisions, creating a more sophisticated system with distributed intelligence.

    -   Human-Agent Collaboration: Human-in-the-loop interactions allow agents to work alongside humans to augment decision-making processes. For instance, agents might prepare data analyses that humans can review and fine-tune, thus improving productivity.

    -   Process Orchestration: Agents can coordinate different tasks across systems, tools, and APIs, helping to automate end-to-end processes like application deployments, cloud orchestration, or even creative processes like writing and design.

### When to use an AI agent?

Using an agent framework for application development provides advantages that are especially beneficial for certain types of applications. While traditional AI models are often used as tools to perform specific tasks (e.g., classification, prediction, or recognition), agents introduce more autonomy, flexibility, and interactivity into the development process.

-   Autonomy and Decision-Making: If your application requires entities that can make independent decisions and adapt to changing conditions (e.g., robotic systems, autonomous vehicles, smart environments), an agent framework is preferable.

-   Multi-Agent Collaboration: If your application involves complex systems that require multiple independent components to work together (e.g., supply chain management, distributed computing, or swarm robotics), agents provide built-in mechanisms for coordination and communication.

-   Interactive and Goal-Oriented: If your application involves goal-driven behavior (e.g., completing tasks autonomously or interacting with users to achieve specific objectives), agent-based frameworks are a better choice. Examples include virtual assistants, game AI, and task planners.

### Add Plugins to an Agent

-   Any Plugin available to an Agent is managed within its respective Kernel instance. This setup enables each Agent to access distinct functionalities based on its specific role.

-   Plugins can be added to the Kernel either before or after the Agent is created. The process of initializing Plugins follows the same patterns used for any Semantic Kernel implementation, allowing for consistency and ease of use in managing AI capabilities.

### Adding Functions to an Agent

-   A Plugin is the most common approach for configuring Function Calling. However, individual functions can also be supplied independently including prompt functions.

## Resources / References

### Recommended

-   [Introduction to Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/)

    -   [Understanding the kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/)
    -   [What is a Plugin?](https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/?pivots=programming-language-csharp)
    -   [Using plugins for Retrieval Augmented Generation (RAG)](https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/using-data-retrieval-functions-for-rag?source=recommendations)

-   [Let's Learn .NET Semantic Kernel - From MS DOTNET - Video](https://www.youtube.com/watch?v=lCQOCoH3Osk)

    -   [Let's Learn .NET Semantic Kernel - Training Module](https://learn.microsoft.com/en-us/collections/4wrote7dxq3mxx?WT.mc_id=dotnet-147962-juyoo)
    -   [Let's Learn .NET Semantic Kernel - Github.IO Instruction (HIGHLY RECOMMEND!)](https://microsoftlearning.github.io/mslearn-ai-semantic-kernel/)
    -   [Let's Learn .NET Semantic Kernel - Github Template](https://github.com/MicrosoftLearning/mslearn-ai-semantic-kernel)

-   [Develop AI Agents with Azure OpenAI and Semantic Kernel SDK - Github Template](https://github.com/MicrosoftLearning/MSLearn-Develop-AI-Agents-with-Azure-OpenAI-and-Semantic-Kernel-SDK)

-   [Semantic Kernel - Microsoft - Github](https://github.com/microsoft/semantic-kernel)

    -   [Semantic Kernel - Samples - Microsoft - Github](https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples)
    -   [Semantic Kernel - Samples - Agents - Microsoft - Github](https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithAgents)

-   [Develop an AI Agent using Semantic Kernel AI-3026 - Youtube (Python)](https://www.youtube.com/watch?v=NFs4zXIhHKU)
-   [Orchestrate a multi-agent solution using Semantic Kernel - Microsoft (Python) - Great Read though](https://learn.microsoft.com/en-us/training/modules/orchestrate-semantic-kernel-multi-agent-solution/)

-   [Building AI Agent Using Semantic Kernel and Agent Framework - Systenic AI - Blog](https://systenics.ai/blog/2025-04-09-building-ai-agent-using-semantic-kernel-agent-framework/)
    -   Content falls inline with `Develop an AI Agent using Semantic Kernel AI-3026 - Youtube (Python)`
-   [Building Multi‑Agent AI Workflows with Semantic Kernel Agent Framework in .NET - Systenic AI - Blog](https://systenics.ai/blog/2025-04-17-building-multiagent-ai-workflows-with-semantic-kernel-in-dotnet/)
-   [Automate Workflows with Microsoft Semantic Kernel Process Framework in .NET T - Systenic AI - Blog](https://systenics.ai/blog/2025-04-18-automate-workflows-with-microsoft-semantic-kernel-process-framework/)

-   [Full Course (Lessons 1-10) AI Agents for Beginners - Microsoft Developer - Youtube (Python)](www.youtube.com/watch?v=OhI005_aJkA)
-   [AI Agents for Beginner - Microsoft Developer - Github (Python)](https://github.com/microsoft/ai-agents-for-beginners)

-   [Building AI Agent Workflows with Semantic Kernel - Microsoft Developer - Youtube (C# and .NET)](https://www.youtube.com/watch?v=3JFKwerYj04)
    -   [Sample of Complete Semantic Kernel Project- Microsoft Developer - Github (C# and .NET)](https://github.com/vicperdana/SemantiClip)

### Others

-   [Semantic Kernel Cookbook](https://github.com/kinfey/SemanticKernelCookBook/tree/main)
-   [Semantic Kernel SDK Playlist - Will Velida](https://www.youtube.com/watch?v=ZIBSD8ECuiw&list=PLvtybS2EHFJiVD-AjgkO-7OXiVbUi_Ia-)
    -   [Using Local Large Language Models in Semantic Kernel - Will Velida](https://www.youtube.com/watch?v=OEQDZLe3slM)
    -   [Creating and using Plugins with the Semantic Kernal SDK - Will Velida](https://www.youtube.com/watch?v=4B3ppx2U8bE)
    -   [Building your first AI Agent with the Semantic Kernel SDK and C# - Will Velida](https://www.youtube.com/watch?v=ZIBSD8ECuiw)
    -   [Giving our AI Agents skills using native functions in Semantic Kernel SDK - Will Velida](https://www.youtube.com/watch?v=g2MuT1V3FYk)
    -   [Introduction to Memories in the Semantic Kernel SDK - Will Velida](https://www.youtube.com/watch?v=nKjVrV23XN4)
-   [Building Local AI Agents: Semantic Kernel and Ollama in C# - Part 1](https://laurentkempe.com/2025/03/01/building-local-ai-agents-semantic-kernel-and-ollama-in-csharp/)
-   [Building Local AI Agents: Semantic Kernel Agent with Functions in C# using Ollama - Part 2](https://laurentkempe.com/2025/03/02/building-local-ai-agents-semantic-kernel-agent-with-functions-in-csharp-using-ollama/)
-   [Build your first AI Agents using Semantic Kernel in C#](https://www.youtube.com/watch?v=4pI-LxK-NwE)
-   [AI Integrations for Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/integrations)
-   [Integrating Model Context Protocol Tools with Semantic Kernel: A Step-by-Step Guide](https://devblogs.microsoft.com/semantic-kernel/integrating-model-context-protocol-tools-with-semantic-kernel-a-step-by-step-guide/)
-   [Building a Model Context Protocol Server with Semantic Kernel](https://devblogs.microsoft.com/semantic-kernel/building-a-model-context-protocol-server-with-semantic-kernel/)
-   [Working wiht multiple models in Semantic Kernel](https://dev.to/stormhub/working-with-multiple-language-models-in-semantic-kernel-31gk)

-   [Semantic Kernel - Handlebar Prompt Templating Format](https://learn.microsoft.com/en-us/semantic-kernel/concepts/prompts/handlebars-prompt-templates?pivots=programming-language-csharp)
-   [Handlebar Templating Guide](https://handlebarsjs.com/guide/)

# Kernel Memory:

-   [Kernel Memory ](https://github.com/microsoft/kernel-memory) is a .NET and C# library from Microsoft that assists with implementing Retrieval Augmented Generation (RAG). RAG is a technique that enables a large language model (LLM) to access external data sources beyond its initial training data to provide more accurate and relevant answers.

-   Kernel Memory operates on a few key concepts:

    -   Embeddings: It uses an embedding library to convert text into numerical representations called dense vectors. These vectors are then used for searching and finding similar information in a database.

    -   Chat Config: This is the configuration for the LLM that will be used to generate the final answer to a user's question.

    -   Vector Database: A database is needed to store the dense vectors created from the original documents. This database is then searched using the dense vector of the user's question to find the most similar information

-   Kernel Memory can be configured to use various services, such as Azure OpenAI, for both embedding generation and the LLM itself. The library is a promising solution for developers working in the C# or .NET environment, offering a simple way to integrate RAG functionality into their applications.

-   Kernel Memory was developed based on feedback and lessons learned from the Semantic Kernel and Semantic Memory projects. It is designed to be a comprehensive solution for long-term memory, providing built-in features that would otherwise have to be developed manually. These features include:

    -   File ingestion: Automatically storing and extracting text from various files.

    -   Data security: Providing a framework for securing your data.

    -   Language agnosticism: While the core codebase is in .NET, it's designed as a service that can be used from any language or platform, including browser extensions and ChatGPT assistants.

-   While related to Semantic Kernel, Kernel Memory focuses on data indexing and retrieval for Retrieval Augmented Generation (RAG). In this context, the "kernel" refers to the core service that handles the efficient indexing of datasets through custom data pipelines and supports RAG capabilities, allowing you to ask questions to an LLM using your own information as a data source. Kernel Memory can also integrate with Semantic Kernel as a plugin, allowing it to act as the vector store layer within a Semantic Kernel application.

-   It will require an embedding model, but it will abstract

-   By default it will store everything in memory

### Workflow of Kernel Memory for RAG

-   The general workflow for using Kernel Memory for RAG is:

    1. A user asks a question.

    2. The question is converted into a dense vector using the embedding library.

    3. The vector database is searched for documents where the dense vector is most similar to the question's vector.

    4. The retrieved documents, along with the original question, are sent to the LLM.

    5. The LLM analyzes the provided information to formulate an answer.

## Types of Kernel Memory

-   Kernel Memory can be run in two modes: serverless mode and web service mode. The choice between these depends on your application's needs for scale, persistence, and language compatibility.

### Serverless Mode

-   Serverless mode embeds a MemoryServerless class directly into your .NET application. It's a quick and easy way to get started with Kernel Memory. 🚀

    -   In-Process Execution: The entire RAG pipeline—from document parsing and embedding generation to search and answer retrieval—runs within the same process as your application.

    -   Volatile by Default: By default, data and embeddings are stored only in memory. This means the knowledge base is temporary and will be lost when the application is restarted. You can, however, configure it to use a persistent vector store if needed.

    -   .NET Only: Because the code is embedded, this mode is limited to .NET applications.

    -   Best for: Prototyping, small-scale applications, or scenarios where the data is short-lived and doesn't need to be shared.

### Web Service Mode

-   Web service mode runs Kernel Memory as a standalone REST API service. Your application communicates with this service via a web client, making it more scalable and flexible.

    -   Asynchronous Operation: The service can handle long-running tasks, such as ingesting thousands of documents, without blocking your main application.

    -   Scalability: The service can be deployed on its own and scaled independently. This is ideal for large, enterprise-level applications with high data ingestion rates.

    -   Language Agnostic: Because it's a web service, it can be consumed by applications written in any language (e.g., Python, C#, Java) using standard HTTP requests.

    -   Best for: Production environments, large datasets, and applications that require a durable, shared, and scalable knowledge base.

## Semantic Memory

-   Semantic Memory is an earlier library for C#, Python, and Java. It was the first iteration of a long-term memory solution for the Semantic Kernel project. Its primary function is to wrap direct calls to databases and support vector search.

    -   Multi-language support: The core library is maintained in three different languages.

    -   Varied support: The list of supported storage engines (connectors) for Semantic Memory varies across the different languages.

-   Kernel Memory is a more robust, feature-rich service built on the foundation of Semantic Memory, offering a unified .NET codebase and a service-oriented approach for broader language compatibility.

### Relationship between Kernel Memory (KM) and Semantic Kernel (SK)

-   Semantic Kernel is an SDK for C#, Python, and Java used to develop solutions with AI. SK includes libraries that wrap direct calls to databases, supporting vector search.

-   Semantic Kernel is maintained in three languages, while the list of supported storage engines (known as "connectors") varies across languages.

-   Kernel Memory (KM) is a SERVICE built on Semantic Kernel, with additional features developed for RAG, Security, and Cloud deployment. As a service, KM can be used from any language, tool, or platform, e.g. browser extensions and ChatGPT assistants.

-   Kernel Memory provides several features out of the scope of Semantic Kernel, that would usually be developed manually, such as storing files, extracting text from documents, providing a framework to secure users' data, content moderation etc.

-   Kernel Memory is also leveraged to explore new AI patterns, which sometimes are backported to Semantic Kernel and Microsoft libraries, for instance vector stores flexible schemas, advanced filtering, authentications.

## Resources / References

-   [Github - Microsoft - Kernel Memory](https://github.com/microsoft/kernel-memory)
-   [Getting started with Kernel Memory](https://www.youtube.com/watch?v=rsW2HTM6tM8)
-   [Document Search in .NET with Kernel Memory](https://www.youtube.com/watch?v=h8bKn1nzjrQ)
-   [Kernal Memory Learn Series Playlist - Will Velida](https://www.youtube.com/watch?v=ZIBSD8ECuiw&list=PLvtybS2EHFJiVD-AjgkO-7OXiVbUi_Ia-)
-   [Kernel Memory: integrate in your code with serverless mode.](https://www.youtube.com/watch?v=DV-YxQSHer4)
-   [Build a custom Copilot experience with your private data using and Kernel Memory](https://www.developerscantina.com/p/kernel-memory/)

# RAG

## OVERVIEW

-   Description

### References and Link

-   [Lenni's Technology Blog - Building a Custom RAG Solution with Azure OpenAI and Multi-Backend Support](https://lennilobel.wordpress.com/2025/03/04/building-a-custom-rag-solution-with-azure-openai-and-multi-backend-support/)
-   [Lenni's Technology Blog - RAG Demo - Github](https://github.com/lennilobel/ai-demos-public/tree/main/Rag)
-   [RAG on your data with .NET, AI and Azure SQL](https://www.youtube.com/watch?v=q9R2m7UIn-o)
-   [Retrieval-Augmented Generation (RAG) with .NET 8: A Full Local Resource Guide ](https://www.youtube.com/watch?v=VVZU-lbEegw)
-   [Ollama C# Playground](https://github.com/elbruno/Ollama-CSharp-Playground)
    -   [Github - Phi Cookbook: Hands-On Examples with Microsoft's Phi Models](https://github.com/microsoft/PhiCookBook)
-   [Build a RAG application with LangChain and Local LLMs powered by Ollama - Blog](https://devblogs.microsoft.com/cosmosdb/build-a-rag-application-with-langchain-and-local-llms-powered-by-ollama/)
-   [Build a RAG application with LangChain and Local LLMs powered by Ollama - Video](https://www.youtube.com/watch?v=AQ-h1JHaX7I)

# COMPONENTS & CONCEPTS OF RAG

### (1) CANONICALIZING

-   Description: This is the process in which we breakdown and clear

#### (1.1) TOKENIZER / TOKENIZATION

-   Before an AI model can process any input, it first converts it into a series of numbers called a vector. This conversion process is known as tokenization.

-   Tokenization involves splitting the input into smaller pieces, referred to as tokens.

-   The tokens of an input is contained in a vector, and each token is then assigned a unique ID from a lookup table.

-   Finally, each ID corresponds to a specific entry in an embedding matrix, which is what the model uses to represent and process the token.

    -   For example, the word "cat" might have the token ID 345, while "mat" might have 563

    -   Generally the tokenizer model will have its predefined matrix.

    -   An AI Model, such as ChatGPT may

-   The semantic relationships between the tokens can be analyzed by using the token ID sequences (or its order).

-   Inputs can be texts, images, audio, or video clips.

-   For text, the input is broken down into words or sub-words.

-   For images, the input is split into small patches, such as 3x3 or 5x5 pixels.

-   For audio, the input is sliced into small time windows and then converted into frequency patterns.

-   For video, the input is broken down into words

-   Since an AI model have a limit on the number of tokens it can take in an input at once, it is the responsibility of the user to partition and breakdown inputs that may exceed token count or some large criteria.

-   In this case, a user may want to estimate the token size of an input, whether its a document file, image, or string of text, and breaking it down into smaller segments.

-   This process is known as chunking.

-   [Tokenization: How AI understands text, images, and more](https://www.youtube.com/watch?app=desktop&v=l8GNSIqbecY)

-   [LLM Tokenizers Explained: BPE Encoding, WordPiece and SentencePiece](https://www.youtube.com/watch?v=hL4ZnAWSyuU)

#### (1.2) CHUNKING

-   AI Models take tokens as

### (2) EMBEDDING & EMBEDDING MODEL

-   Embedding is the process of further take each token and further breakin it's individual vector matrix

-   The matrix presents a key-value relationship of that token's association with

-   [Building Pico: A GPT-4 Based Assistant That Can Interact With The World - Code Explained](https://www.youtube.com/watch?v=6pF5CqbEYv4&t=1875s)

### (3) VECTOR DATABASE

-   A VECTOR DATABASE STORES

#### (3.1) SETTING UP A VECTOR DATABASE

#### (3.2) INSERTING INTO A VECTOR DATABASE

#### (3.3) QUERYING INTO A VECTOR DATABASE

##### (3.3.1) SEMANTIC SEARCH ALGORITHM

# Modal Context Protocol

## Resources / References

[Let's Learn MCP: C# - Video](https://www.youtube.com/watch?v=4zkIBMFdL2w&list=PLdo4fOcmZ0oVGRpRwbMhUA0KAvMA2mLyN&index=1)
[Let's Learn MCP with C# - Tutorial Series](https://github.com/microsoft/lets-learn-mcp-csharp)
[Building a Model Context Protocol Server with .NET and Semantic Kernel Integration - Systenics AI Blog](https://systenics.ai/blog/2025-04-10-building-a-model-context-protocol-server-with-net-and-semantic-kernel-integration/)
