import { useState, type SubmitEvent } from "react";
import { api, ApiError } from "../api/client";
import type { Comment } from "../api/types";

type Props = {
    ticketId: number
    canWriteInternal: boolean
    onAdded: (comment: Comment) => void
}

export default function CommentForm({ ticketId, canWriteInternal, onAdded }: Props) {
    const [text, setText] = useState('')
    const [internal, setInternal] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const [busy, setBusy] = useState(false)

    async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
        e.preventDefault()
        setError(null)
        setBusy(true)
        try {
            const comment = await api<Comment>(`/tickets/${ticketId}/comments`, {
                method: 'POST',
                body: { text, isInternal: internal },
            })
            onAdded(comment)
            setText('')
            setInternal(false)
        } catch(err) {
            setError(err instanceof ApiError ? err.message : 'Cannot reach the server.')
        } finally {
            setBusy(false)
        }
    }
    return (
        <form onSubmit={handleSubmit}>
            <textarea
                rows={3}
                value={text}
                onChange={e => setText(e.target.value)}
                placeholder="Write a comment..."
                required
                maxLength={4000}
            />
            {error && <p className="error">{error}</p>}
            <div className="form-actions">
                <button className="primary" type="submit" disabled={busy}>Add comment</button>
                {canWriteInternal && (
                    <label className="checkbox">
                        <input type="checkbox" checked={internal} onChange={e => setInternal(e.target.checked)} />
                        Internal note (hidden from the employee)
                    </label>
                )}
            </div>
        </form>
    )
}