# @decorators

This project combines **AR Foundation** with **ARCore** in Unity to create augmented reality experiences. The backend is powered by **FastAPI**, providing a robust API for interaction with Unity.

---

## 📥 Download and Setup

1. **Download the project**:  
   You can download the project from [here](<link_to_project_download>).

2. **Navigate to the decorators server directory**:  
   Once you've downloaded and extracted the project, open your terminal and change to the `@decorators_server` directory:
   ```bash
   cd @decorators_server
   

---

## 🚀 Running the FastAPI Server

1. **Start the server**:  
   To start the FastAPI backend, run the `start.sh` script:
   ```bash
   ./start.sh
   ```

2. **Default port**:  
   The server will run on port `5000` by default. You can access the following endpoints in your browser:

   - **Swagger UI**:  
     [http://127.0.0.1:5000/docs](http://127.0.0.1:5000/docs) - Interactive API documentation.
   
   - **ReDoc UI**:  
     [http://127.0.0.1:5000/redoc](http://127.0.0.1:5000/redoc) - Detailed API documentation.
   
   - **Root Endpoint**:  
     [http://127.0.0.1:5000](http://127.0.0.1:5000) - The main server endpoint.

3. **Change the port** (optional):  
   If you'd like to change the default port (5000), update the `start.sh` script or specify the port manually when starting the server:
   ```bash
   ./start.sh --port <custom_port>
   ```

---

## 🛠️ Requirements

### Backend
- **Python 3.10+**
- **FastAPI**
- **Dependencies**: Install the required Python packages:
  ```bash
  pip install -r requirements.txt
  ```

### Unity
- **Unity Version**: 2021.3 or later (with **AR Foundation** and **ARCore** plugins installed)
  
### Additional Setup
- Follow Unity's official setup for **AR Foundation** and **ARCore** to configure your Unity environment for augmented reality.

---

## 🌟 Features

- **ARCore Integration**: Build interactive AR experiences using ARCore in Unity.
- **FastAPI Backend**: Handle backend data and services with FastAPI.
- **Swagger UI**: Test your API interactively with the provided Swagger UI.
- **ReDoc**: Alternative API documentation for detailed usage.

---

## 📂 Project Structure

```
@decorators_server/     # FastAPI backend server code
    start.sh            # Script to start the FastAPI server
    app/                # FastAPI application files (e.g., routes, logic)
UnityProject/           # Unity project for AR Foundation with ARCore
    Assets/             # Unity assets and resources
    Scenes/             # Unity scenes for AR applications
```
---

## 🏁 Getting Started

1. Open the `UnityProject/` folder in **Unity**.
2. Make sure **AR Foundation** and **ARCore** plugins are installed.
3. Follow the Unity documentation to set up your AR project.
4. Deploy the AR app to a supported Android device.
5. Run the FastAPI backend as described above to handle any server-side interactions.

---

## 🤝 Contributing

Feel free to fork the project and submit a pull request with your improvements or bug fixes. If you encounter any issues or have suggestions, open an issue on GitHub.

---

## 📜 License

This project is licensed under the [MIT License](LICENSE).
```


### Key Points:

1. **Tailored Furniture Recommender System**: We use various AI techniques and metrics for recommending furniture as per the user's preference as well as the suitability to the selected area 
2. **AR Foundation and ARCore**: Unity setup with AR Foundation and ARCore for building augmented reality experiences.
3. **Swagger UI**: The FastAPI app automatically generates Swagger UI at `http://127.0.0.1:5000/docs` for interactive API testing.
4. **ReDoc**: For more detailed API documentation, ReDoc is available at `http://127.0.0.1:5000/redoc`.
5. **Port Configuration**: The default port is `5000`, but it can be changed by passing a custom port argument when running the server.

This `README.md` should help users quickly set up and run the project, as well as provide details on the FastAPI server and Unity setup for ARCore.

```
