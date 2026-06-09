import requests

BASE_URL = "http://localhost:10000"
LOGIN_ENDPOINT = f"{BASE_URL}/api/auth/login"
TIMEOUT = 30

def test_postapiauthloginwithvalidcredentials():
    users = [
        {"email": "admin@pharmacy.com", "password": "admin123"},
        {"email": "supervisor@pharmacy.com", "password": "super123"}
    ]

    headers = {
        "Content-Type": "application/json"
    }

    for user in users:
        try:
            response = requests.post(
                LOGIN_ENDPOINT,
                json={"email": user["email"], "password": user["password"]},
                headers=headers,
                timeout=TIMEOUT
            )
        except requests.RequestException as e:
            assert False, f"Request failed: {e}"

        # Assert status code 200
        assert response.status_code == 200, f"Expected status 200 but got {response.status_code} for user {user['email']}"

        # Assert response contains a JWT token (usually in a field like 'token' or 'jwt')
        try:
            json_response = response.json()
        except ValueError:
            assert False, f"Response is not valid JSON for user {user['email']}"

        token = json_response.get("token") or json_response.get("jwt") or json_response.get("access_token")
        assert token is not None and isinstance(token, str) and len(token) > 0, f"JWT token not found or invalid for user {user['email']}"

test_postapiauthloginwithvalidcredentials()