import api from './axios'
import type { AuthResponse, User } from '../types'

export const login = (identifier: string, password: string) =>
  api.post<AuthResponse>('/auth/login', { identifier, password }).then((r) => r.data)

export const getProfile = () => api.get<User>('/profile').then((r) => r.data)

export interface ProfileForm {
  name: string
  email: string
  // Only sent when the editor is an administrator; the server ignores them for regular users.
  personType?: 'Fisica' | 'Juridica'
  cedulaRnc?: string
  expenseLimit?: number
  cutDay?: number
}

export const updateProfile = (data: ProfileForm) =>
  api.put<User>('/profile', data).then((r) => r.data)

export const changePassword = (data: { currentPassword: string; newPassword: string }) =>
  api.put('/profile/password', data)
