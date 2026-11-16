import type { PaginationMetadata } from "./types"

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

let sessionExpiredHandler: (() => void) | null = null

export function onSessionExpired(handler: (() => void) | null) {
    sessionExpiredHandler = handler
}

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

async function request(path: string, options: RequestOptions = {}): Promise<Response> {
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

        if (res.status === 401 && !path.startsWith('/auth/')) {
            if (await refresh()) {
                res = await send()
            } else {
                accessToken = null
                sessionExpiredHandler?.()
            }
        }

        if (!res.ok) {
            const body = (await res.json().catch(() => null)) as ProblemDetails | null
            throw new ApiError(res.status, body)
        }

    return res
    }

    export async function api<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const res = await request(path, options)
    if (res.status === 204) return undefined as T
    return (await res.json()) as T
}

export type Paged<T> = { items: T[], meta: PaginationMetadata }

export async function apiPaged<T>(path: string): Promise<Paged<T>> {
    const res = await request(path)
    const meta = JSON.parse(res.headers.get('Pagination') ?? 'null') as PaginationMetadata
    return { items: (await res.json()) as T[], meta }
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