/**
 * StatusBar Component Tests
 */
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { StatusBar } from '../StatusBar';

describe('StatusBar', () => {
  it('should render without crashing', () => {
    render(<StatusBar isDirty={false} />);
    expect(screen.getByText('⌨️')).toBeInTheDocument();
  });

  it('should show unsaved indicator when dirty', () => {
    render(<StatusBar isDirty={true} />);
    expect(screen.getByText('Unsaved')).toBeInTheDocument();
  });

  it('should not show unsaved indicator when not dirty', () => {
    render(<StatusBar isDirty={false} />);
    expect(screen.queryByText('Unsaved')).not.toBeInTheDocument();
  });

  it('should display workspace path when provided', () => {
    const path = '/home/user/workspace';
    render(<StatusBar isDirty={false} workspacePath={path} />);
    expect(screen.getByTitle(path)).toBeInTheDocument();
  });

  it('should show validation errors', () => {
    render(<StatusBar isDirty={false} validationErrors={3} />);
    expect(screen.getByText('🔴')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
  });

  it('should show validation warnings', () => {
    render(<StatusBar isDirty={false} validationWarnings={2} />);
    expect(screen.getByText('🟡')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('should display character count', () => {
    render(<StatusBar isDirty={false} characterCount={150} />);
    expect(screen.getByText('150 chars')).toBeInTheDocument();
  });

  it('should display word count', () => {
    render(<StatusBar isDirty={false} wordCount={25} />);
    expect(screen.getByText('25 words')).toBeInTheDocument();
  });

  it('should toggle keyboard shortcuts panel', async () => {
    const user = userEvent.setup();
    render(<StatusBar isDirty={false} />);

    const shortcutsButton = screen.getByTitle('Show keyboard shortcuts');
    await user.click(shortcutsButton);

    // Check if shortcuts panel appears
    expect(screen.getByText('Keyboard Shortcuts')).toBeInTheDocument();

    // Click close button
    const closeButton = screen.getByRole('button', { name: 'Close' });
    await user.click(closeButton);

    // Panel should be hidden
    expect(screen.queryByText('Keyboard Shortcuts')).not.toBeInTheDocument();
  });

  it('should display keyboard shortcuts when opened', async () => {
    const user = userEvent.setup();
    render(<StatusBar isDirty={false} />);

    const shortcutsButton = screen.getByTitle('Show keyboard shortcuts');
    await user.click(shortcutsButton);

    // Check for some common shortcuts
    expect(screen.getByText('Save changes')).toBeInTheDocument();
    expect(screen.getByText('Open workspace')).toBeInTheDocument();
  });

  it('should handle all status props together', () => {
    render(
      <StatusBar
        isDirty={true}
        workspacePath="/workspace"
        validationErrors={1}
        validationWarnings={2}
        characterCount={100}
        wordCount={20}
      />
    );

    expect(screen.getByText('Unsaved')).toBeInTheDocument();
    expect(screen.getByTitle('/workspace')).toBeInTheDocument();
    expect(screen.getByText('🔴')).toBeInTheDocument();
    expect(screen.getByText('🟡')).toBeInTheDocument();
    expect(screen.getByText('100 chars')).toBeInTheDocument();
    expect(screen.getByText('20 words')).toBeInTheDocument();
  });
});
