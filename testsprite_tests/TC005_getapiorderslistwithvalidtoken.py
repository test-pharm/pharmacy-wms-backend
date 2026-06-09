import requests

BASE_URL = "http://localhost:10000"
LOGIN_ENDPOINT = f"{BASE_URL}/api/auth/login"
ORDERS_ENDPOINT = f"{BASE_URL}/api/orders"
TIMEOUT = 30

def test_get_api_orders_list_with_valid_token():
    # Step 1: Authenticate with valid credentials to get JWT token
    login_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    try:
        login_response = requests.post(LOGIN_ENDPOINT, json=login_payload, timeout=TIMEOUT)
        assert login_response.status_code == 200, f"Login failed with status {login_response.status_code}"
        login_json = login_response.json()
        token = login_json.get("token") or login_json.get("accessToken")
        assert token, "JWT token not found in login response"

        # Step 2: Use JWT token to get the orders list
        headers = {
            "Authorization": f"Bearer {token}"
        }
        orders_response = requests.get(ORDERS_ENDPOINT, headers=headers, timeout=TIMEOUT)
        assert orders_response.status_code == 200, f"GET /api/orders failed with status {orders_response.status_code}"
        orders_json = orders_response.json()
        # Validate that the response is a list or contains a list of warehouse stock movements
        assert isinstance(orders_json, list) or (
            isinstance(orders_json, dict) and 
            any(isinstance(v, list) for v in orders_json.values())
        ), "Response does not contain a list of warehouse stock movements"
    except requests.RequestException as e:
        assert False, f"Request failed: {e}"

test_get_api_orders_list_with_valid_token()