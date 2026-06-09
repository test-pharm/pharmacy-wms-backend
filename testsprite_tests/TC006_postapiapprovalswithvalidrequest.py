import requests
import datetime

BASE_URL = "http://localhost:10000"
TIMEOUT = 30

def test_postapiapprovalswithvalidrequest():
    # Login as admin to get JWT token
    login_url = f"{BASE_URL}/api/auth/login"
    admin_credentials = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    login_resp = requests.post(login_url, json=admin_credentials, timeout=TIMEOUT)
    assert login_resp.status_code == 200, f"Admin login failed: {login_resp.text}"
    token = login_resp.json().get("token")
    assert token, "JWT token missing in admin login response"

    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    product_id = None
    try:
        # Get list of categories to get valid categoryId (needed for product creation)
        categories_resp = requests.get(f"{BASE_URL}/api/categories", headers=headers, timeout=TIMEOUT)
        assert categories_resp.status_code == 200, f"Failed to get categories: {categories_resp.text}"
        categories = categories_resp.json()
        assert isinstance(categories, list) or "data" in categories, "Categories response format unexpected"
        # Categories could be in list directly or under 'data'
        category_list = categories.get("data") if "data" in categories else categories
        assert category_list and len(category_list) > 0, "No categories found to assign product categoryId"
        category_id = category_list[0].get("id") if isinstance(category_list[0], dict) else category_list[0]
        assert category_id is not None, "CategoryId missing"

        # Create a product with required fields including quantity to generate batch
        product_payload = {
            "materialName": "Test Material",
            "materialSku": f"SKU{int(datetime.datetime.utcnow().timestamp())}",
            "quantity": 10,
            "unit": "box",
            "logNumber": "LOG12345",
            "supplier": "Test Supplier Ltd.",
            "categoryId": category_id
        }
        product_resp = requests.post(f"{BASE_URL}/api/products", headers=headers, json=product_payload, timeout=TIMEOUT)
        assert product_resp.status_code == 201, f"Product creation failed: {product_resp.text}"
        product_data = product_resp.json()
        product_id = product_data.get("id")
        assert product_id is not None, "Product ID missing in creation response"
        batches = product_data.get("batches")
        assert isinstance(batches, list) and len(batches) > 0, "Product batches missing or empty"
        batch_id = batches[0].get("id")
        assert batch_id is not None, "Batch ID missing in product batches"

        # Prepare payload for POST /api/approvals using batchId, newExpiry, and reason
        new_expiry_date = (datetime.date.today() + datetime.timedelta(days=365)).isoformat()
        approval_payload = {
            "batchId": batch_id,
            "newExpiry": new_expiry_date,
            "reason": "Extend expiry for quality assurance review"
        }

        # Post approval request
        approval_resp = requests.post(f"{BASE_URL}/api/approvals", headers=headers, json=approval_payload, timeout=TIMEOUT)
        assert approval_resp.status_code == 200, f"Approval request failed: {approval_resp.text}"
        approval_data = approval_resp.json()
        assert isinstance(approval_data, dict), "Approval response is not a JSON object"

        # Check the status is Pending in response (assuming response has 'status' key)
        status = approval_data.get("status")
        assert status is not None, "Approval response missing 'status' field"
        assert status.lower() == "pending", f"Expected status 'Pending', got '{status}'"

    finally:
        # Clean-up: Delete created product to avoid clutter
        if product_id is not None:
            requests.delete(f"{BASE_URL}/api/products/{product_id}", headers=headers, timeout=TIMEOUT)

test_postapiapprovalswithvalidrequest()