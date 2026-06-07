import requests
import datetime

BASE_URL = "http://localhost:10000"
TIMEOUT = 30


def login(email: str, password: str) -> str:
    url = f"{BASE_URL}/api/auth/login"
    payload = {"email": email, "password": password}
    response = requests.post(url, json=payload, timeout=TIMEOUT)
    assert response.status_code == 200, f"Login failed for {email}: {response.text}"
    data = response.json()
    token = data.get("token")
    assert token, "No token received after login"
    return token


def create_category(token: str) -> int:
    url = f"{BASE_URL}/api/categories"
    headers = {"Authorization": f"Bearer {token}"}
    payload = {"name": "TestCategoryApprovals"}
    response = requests.post(url, json=payload, headers=headers, timeout=TIMEOUT)
    # As category creation returns 201 Created
    assert response.status_code == 201 or response.status_code == 200, f"Category creation failed: {response.text}"
    data = response.json()
    category_id = data.get("id")
    assert category_id is not None, "Category ID not found in response"
    return category_id


def create_product(token: str, category_id: int) -> dict:
    url = f"{BASE_URL}/api/products"
    headers = {"Authorization": f"Bearer {token}"}
    # According to instructions, include 'materialName', 'materialSku', 'quantity', 'unit', 'logNumber', 'supplier', and 'categoryId'
    payload = {
        "materialName": "TestMaterialForApproval",
        "materialSku": "SKU0001-APPROVAL",
        "quantity": 10,  # Must include quantity so batch is generated
        "unit": "boxes",
        "logNumber": "LOG123456",
        "supplier": "TestSupplier",
        "categoryId": category_id
    }
    response = requests.post(url, json=payload, headers=headers, timeout=TIMEOUT)
    assert response.status_code == 201, f"Product creation failed: {response.text}"
    product_data = response.json()
    assert "batches" in product_data and isinstance(product_data["batches"], list) and product_data["batches"], "No batches found in product response"
    return product_data


def delete_product(token: str, product_id: int):
    url = f"{BASE_URL}/api/products/{product_id}"
    headers = {"Authorization": f"Bearer {token}"}
    response = requests.delete(url, headers=headers, timeout=TIMEOUT)
    # Delete may return 204 No Content (per PRD) or another success status
    assert response.status_code in [204, 200, 404], f"Failed to delete product id {product_id}: {response.status_code}"


def post_approval(token: str, batch_id: int, new_expiry: str, reason: str) -> dict:
    url = f"{BASE_URL}/api/approvals"
    headers = {"Authorization": f"Bearer {token}"}
    payload = {
        "batchId": batch_id,
        "newExpiry": new_expiry,
        "reason": reason
    }
    response = requests.post(url, json=payload, headers=headers, timeout=TIMEOUT)
    # Instruction says check for status 200 (not 201)
    assert response.status_code == 200, f"Approval post failed: {response.status_code} {response.text}"
    data = response.json()
    return data


def test_postapiapprovalswithvalidrequest():
    # Login as Admin to get token (Admin role)
    admin_token = login("admin@pharmacy.com", "admin123")

    # Create a category first to use with product
    category_id = create_category(admin_token)

    product_id = None
    try:
        # Create product with batch (quantity=10) - to get batchId from product's batches[0].id
        product_data = create_product(admin_token, category_id)
        product_id = product_data.get("id")
        batch_id = product_data["batches"][0]["id"]
        assert isinstance(batch_id, int), "Batch ID is not an integer"

        # Prepare newExpiry - e.g. one year after today in YYYY-MM-DD
        new_expiry = (datetime.datetime.utcnow() + datetime.timedelta(days=365)).strftime("%Y-%m-%d")
        reason = "Routine expiry update per QA policy"

        # Post approval request
        approval_response = post_approval(admin_token, batch_id, new_expiry, reason)

        # Validate approval response contains status Pending and relevant info
        assert "status" in approval_response, "Approval response missing 'status'"
        assert approval_response["status"].lower() == "pending", f"Expected status 'Pending', got '{approval_response['status']}'"
        assert approval_response.get("batchId") == batch_id, "Batch ID mismatch in approval response"
        assert approval_response.get("newExpiry") == new_expiry, "newExpiry mismatch in approval response"
        assert approval_response.get("reason") == reason, "Reason mismatch in approval response"

    finally:
        # Cleanup - delete product
        if product_id:
            delete_product(admin_token, product_id)


test_postapiapprovalswithvalidrequest()