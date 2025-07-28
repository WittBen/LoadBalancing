# C# Load Balancer Demonstrations

This project is a C# console application designed to demonstrate various load-balancing algorithms in a simulated environment. It provides a visual and interactive way to understand the fundamental principles and differences between Round-Robin, Weighted, and Adaptive load balancing.

## ✨ Features

- **Interactive Menu**: Easily select the load-balancing strategy you want to demonstrate.
- **Three Load-Balancing Algorithms**:
    1.  **Round-Robin**: Distributes requests sequentially across servers.
    2.  **Weighted**: Distributes requests based on assigned server weights.
    3.  **Adaptive (Least Connections/Fastest Response)**: Intelligently distributes requests based on the historical performance (latency and failure rate) of the servers.
- **Simulated Server Environment**: A pool of mock servers with different characteristics (fast, slow, unreliable) to realistically test the behavior of the load balancers.
- **Visual Feedback**: Each request and its outcome are clearly displayed and color-coded in the console, making the load balancer's decision-making process easy to follow.

## 🚀 Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (Version 6, 7, 8, or higher)
- Git (optional, for cloning the repository)

### Running the Application

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/WittBen/LoadBalancing.git
    ```

2.  **Run the application:**
    Use the `dotnet run` command in the project's root directory.
    ```bash
    dotnet run
    ```

3.  **Interact with the menu:**
    Once started, a menu will appear in the console. Enter `1`, `2`, or `3` to start the respective demonstration. Use `q` to quit the program.

## 📊 Demonstrations Explained

The application simulates 25 requests to a load balancer, which forwards them to three backend servers.

#### Simulated Servers

-   **`http://server-a`**: **Fast & Reliable** (response time: 50ms)
-   **`http://server-b`**: **Slow & Reliable** (response time: 400ms)
-   **`http://server-c`**: **Unreliable** (medium speed, but every 3rd request fails)

---

### 1. Round-Robin Load Balancing

-   **Principle**: Requests are distributed strictly in order to servers A, B, and C. After C, the cycle starts over with A.
-   **Observation**: You will see requests being distributed evenly (`A -> B -> C -> A -> ...`), regardless of server performance or health. Failed requests to Server C are marked accordingly.

---

### 2. Weighted Load Balancing

-   **Principle**: Servers with a higher weight receive proportionally more requests. In this demo, the weights are configured as follows:
    -   Server A: **Weight 5**
    -   Server B: **Weight 2**
    -   Server C: **Weight 3**
-   **Observation**: After 25 requests, Server A will have received the most requests, followed by Server C, and finally Server B. The distribution reflects the predefined weights.

---

### 3. Adaptive Load Balancing

-   **Principle**: The load balancer continuously measures the performance (average latency and failure rates) of each server. For each new request, it selects the server that is currently the "healthiest" (low latency, no failures).
-   **Observation**:
    -   Initially, requests are distributed among the servers, but the load balancer "learns" quickly.
    -   It will start to avoid the slow **Server B** and the unreliable **Server C**.
    -   After a short time, almost all traffic will be directed to the fast and reliable **Server A**. The metrics displayed before each decision make this learning process visible.

## 📂 Project Structure

The project is organized into logical namespaces to improve readability and maintainability.

 ```bash
├── Program.cs # Main entry point with the interactive menu
│
├── LoadBalancerTypes/
│ ├── RoundRobinLoadBalancer.cs # Implementation of the Round-Robin algorithm
│ ├── WeightedLoadBalancer.cs # Implementation of the Weighted algorithm
│ └── AdaptiveLoadBalancer.cs # Implementation of the Adaptive algorithm
│
├── Server/
│ └── MockBackendServer.cs # Simulates the behavior of the backend servers
│
└── Helpers/
└── DrawerHelper.cs # Helper class for visual output in the console
 ```
