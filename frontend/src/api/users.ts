import api from './axios'
import type { User, UserForm } from '../types'

export const getUsers = () => api.get<User[]>('/users').then((r) => r.data)

export const createUser = (data: UserForm) => api.post<User>('/users', data).then((r) => r.data)

export const updateUser = (id: string, data: Omit<UserForm, 'password'>) =>
  api.put<User>(`/users/${id}`, data).then((r) => r.data)

export const setUserStatus = (id: string, isActive: boolean) =>
  api.patch<User>(`/users/${id}/status`, { isActive }).then((r) => r.data)
