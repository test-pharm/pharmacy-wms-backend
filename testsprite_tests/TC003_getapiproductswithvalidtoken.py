import requests

BASE_URL = "http://localhost:10000"


def test_get_api_products_with_valid_token():
    login_url = f"{BASE_URL}/api/auth/login"
    products_url = f"{BASE_URL}/api/products"
    login_payload = {"email": "admin@pharmacy.com", "password": "admin123"}

    try:
        # Authenticate and get JWT token
        login_resp = requests.post(login_url, json=login_payload, timeout=30)
        assert login_resp.status_code == 200, f"Login failed with status {login_resp.status_code}"
        login_data = login_resp.json()
        token = login_data.get("token") or login_data.get("jwt") or login_data.get("accessToken")
        assert token, "JWT token not found in login response"

        headers = {"Authorization": f"Bearer {token}"}

        # GET /api/products with valid token
        products_resp = requests.get(products_url, headers=headers, timeout=30)
        assert products_resp.status_code == 200, f"GET /api/products failed with status {products_resp.status_code}"
        products_data = products_resp.json()
        # Validate that the response contains a list (could be empty, but is a list)
        assert isinstance(products_data, list) or (
            isinstance(products_data, dict) and any(
                isinstance(products_data.get(key), list) for key in ["data", "items"]
            )
        ), "Products response is not a list or does not contain a list under expected keys"

    except requests.RequestException as e:
        assert False, f"Request failed: {e}"


test_get_api_products_with_valid_token()