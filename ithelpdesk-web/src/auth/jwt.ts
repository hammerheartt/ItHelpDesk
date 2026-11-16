export type Role = 'Employee' | 'Technician' | 'Admin'

export type CurrentUser = {
    id: string
    email: string
    role: Role
}

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'

export function readUser(token: string): CurrentUser {
    const part = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    const payload = JSON.parse(atob(part))
    return { id: payload.sub, email: payload[NAME_CLAIM], role: payload[ROLE_CLAIM]}
}