type ProblemDetails = {
    title?: string
    detail?: string
    errors?: Record<string, string[]>
}

export class ApiError extends Error {
    status: number
    errors?: Record<string, string[]>

    constructor(status: number, body: ProblemDetails | null){
        const firstFieldError = body?.errors ? Object.values(body.errors)[0]?.[0] : undefined
        super (body?.detail ?? firstFieldError ?? body?.title ?? `Request failed (${status})`)
        this.status = status
        this.errors = body?.errors
    }
}

let accessToken: string | null = null
let refreshing: Promise<boolean> | null = null

function refresh(): Promise<boolean> {
    if (!refreshing){
        refreshing = fetch('/api/auth/refresh', { method: 'POST'})
            .then(async res => {
                if (!res.ok) return false
                const data = (await res.json()) as { accessToken: string }
                accessToken = data.accessToken
                return true
            })
            .catch(() => false)
            .finally(() => refreshing = null)
    }
    return refreshing
}

type RequestOptions = { method?: string; body?: unknown }

export async function api<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const send = () =>
        fetch('/api' + path, {
            method: options.method ?? 'GET',
            headers: {
                ...(options.body !== undefined ? { 'Content-Type': 'application/json' } : {}),
                ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
            },
            body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
        })

        let res = await send()

        if (res.status === 401 && !path.startsWith('/auth/') && (await refresh())) {
            res = await send()
        }

        if (!res.ok) {
            const body = (await res.json().catch(() => null)) as ProblemDetails | null 
            throw new ApiError(res.status, body)
        }

        if (res.status === 204) return undefined as T
        return (await res.json()) as T
}

export async function login(email: string, password: string): Promise<string> {
    const data = await api<{ accessToken: string}>('/auth/login', {
        method: 'POST',
        body: { email, password },
    })
    accessToken = data.accessToken
    return data.accessToken
}

export async function restoreSession(): Promise<string | null> {
    return (await refresh()) ? accessToken : null
}

export async function logout(): Promise<void> {
    await api<void>('/auth/logout', { method: 'POST' }).catch(() => {})
    accessToken = null
}