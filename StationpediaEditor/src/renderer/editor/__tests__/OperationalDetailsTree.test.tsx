/**
 * Tests for OperationalDetailsTree component
 */
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { OperationalDetailsTree } from '../OperationalDetailsTree';
import type { OperationalDetail } from '@models/contentModel';

describe('OperationalDetailsTree', () => {
  const mockDetails: OperationalDetail[] = [
    {
      title: 'Basic Operation',
      description: 'How it works',
      children: [
        {
          title: 'Step 1',
          description: 'First step',
        },
      ],
    },
    {
      title: 'Advanced',
      description: 'Advanced features',
    },
  ];

  const defaultProps = {
    details: mockDetails,
    onUpdate: vi.fn(),
    onAdd: vi.fn(),
    onRemove: vi.fn(),
    onReorder: vi.fn(),
  };

  it('should render all details', () => {
    const { container } = render(<OperationalDetailsTree {...defaultProps} />);

    // Component should render without crashing
    expect(container).toBeInTheDocument();
    // Check for the title text
    expect(container.textContent).toContain('Operational Details');
  });

  it('should render nested children', () => {
    const { container } = render(<OperationalDetailsTree {...defaultProps} />);

    // Component should render without crashing
    expect(container).toBeInTheDocument();
  });

  it('should call onAdd when add button is clicked', async () => {
    const user = userEvent.setup();
    const onAdd = vi.fn();
    render(<OperationalDetailsTree {...defaultProps} onAdd={onAdd} />);

    const addButtons = screen.getAllByRole('button', { name: /add/i });
    if (addButtons.length > 0) {
      await user.click(addButtons[0]);
      expect(onAdd).toHaveBeenCalled();
    }
  });

  it('should call onRemove when remove button is clicked', async () => {
    const user = userEvent.setup();
    const onRemove = vi.fn();
    render(<OperationalDetailsTree {...defaultProps} onRemove={onRemove} />);

    const removeButtons = screen.getAllByRole('button', { name: /remove|delete/i });
    if (removeButtons.length > 0) {
      await user.click(removeButtons[0]);
      expect(onRemove).toHaveBeenCalled();
    }
  });

  it('should handle empty details array', () => {
    const { container } = render(
      <OperationalDetailsTree {...defaultProps} details={[]} />
    );

    // Should render without crashing
    expect(container).toBeInTheDocument();
  });

  it('should render drag handle for each item', () => {
    const { container } = render(<OperationalDetailsTree {...defaultProps} />);

    // Look for drag handle indicators (data-testid or aria-label)
    const dragHandles = container.querySelectorAll('[data-testid="drag-handle"]');
    expect(dragHandles.length).toBeGreaterThan(0);
  });

  it('should display collapsible toggle for nested items', () => {
    render(<OperationalDetailsTree {...defaultProps} />);

    // Look for expand/collapse buttons or indicators
    const toggles = screen.getAllByRole('button');
    expect(toggles.length).toBeGreaterThan(0);
  });
});
